using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;
using WorkoutTracker.Application.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Persistence;
using WorkoutTracker.Persistence.UnitOfWork;

namespace WorkoutTracker.Tests.Services;

public sealed class WorkoutPlanServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly WorkoutPlanService _sut;

    public WorkoutPlanServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"workout_plans_tests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _sut = new WorkoutPlanService(_unitOfWork);
        Seed();
    }

    private void Seed()
    {
        var u1 = new User
        {
            Email = "owner@test.local",
            Username = "owner",
            PasswordHash = "h",
            PasswordSalt = "s"
        };
        var u2 = new User
        {
            Email = "other@test.local",
            Username = "other",
            PasswordHash = "h",
            PasswordSalt = "s"
        };
        _context.Users.AddRange(u1, u2);
        _context.Exercises.Add(new Exercise
        {
            Name = "Bench Press",
            Category = ExerciseCategory.Strength,
            MuscleGroup = MuscleGroup.Chest
        });
        _context.SaveChanges();
    }

    private int OwnerUserId => _context.Users.Single(u => u.Username == "owner").Id;
    private int OtherUserId => _context.Users.Single(u => u.Username == "other").Id;
    private int ExerciseId => _context.Exercises.Single().Id;

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Create_and_list_returns_plan_for_owner_only()
    {
        var created = await _sut.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "Push" });
        Assert.True(created.Succeeded);
        Assert.Equal("Push", created.Value!.Name);

        var list = await _sut.ListAsync(OwnerUserId);
        Assert.True(list.Succeeded);
        var plans = list.Value!;
        Assert.Single(plans);
        Assert.Equal(0, plans[0].ExerciseCount);

        var otherList = await _sut.ListAsync(OtherUserId);
        Assert.True(otherList.Succeeded);
        Assert.Empty(otherList.Value!);
    }

    [Fact]
    public async Task GetById_denies_other_user_with_not_found()
    {
        var created = await _sut.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "Legs" });
        var planId = created.Value!.Id;

        var foreign = await _sut.GetByIdAsync(OtherUserId, planId);
        Assert.False(foreign.Succeeded);
        Assert.Equal("not_found", foreign.FailureCode);
    }

    [Fact]
    public async Task AddExercise_rejects_unknown_exercise()
    {
        var created = await _sut.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "PlanA" });
        var planId = created.Value!.Id;

        var bad = await _sut.AddExerciseAsync(
            OwnerUserId,
            planId,
            new AddPlanExerciseRequest
            {
                ExerciseId = 999_999,
                TargetSets = 3,
                TargetReps = 10,
                OrderIndex = 0
            });

        Assert.False(bad.Succeeded);
        Assert.Equal("validation_failed", bad.FailureCode);
    }

    [Fact]
    public async Task AddExercise_rejects_duplicate_exercise_in_same_plan()
    {
        var created = await _sut.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "PlanB" });
        var planId = created.Value!.Id;

        var first = await _sut.AddExerciseAsync(
            OwnerUserId,
            planId,
            new AddPlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 3,
                TargetReps = 10,
                OrderIndex = 0
            });
        Assert.True(first.Succeeded);

        var duplicate = await _sut.AddExerciseAsync(
            OwnerUserId,
            planId,
            new AddPlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 4,
                TargetReps = 8,
                OrderIndex = 1
            });

        Assert.False(duplicate.Succeeded);
        Assert.Equal("duplicate_exercise", duplicate.FailureCode);
    }

    [Fact]
    public async Task Update_and_remove_plan_exercise_round_trip()
    {
        var created = await _sut.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "PlanC" });
        var planId = created.Value!.Id;

        var added = await _sut.AddExerciseAsync(
            OwnerUserId,
            planId,
            new AddPlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 2,
                TargetReps = 12,
                OrderIndex = 0
            });
        var lineId = added.Value!.Exercises[0].Id;

        var updated = await _sut.UpdatePlanExerciseAsync(
            OwnerUserId,
            planId,
            lineId,
            new UpdatePlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 5,
                TargetReps = 5,
                OrderIndex = 1
            });
        Assert.True(updated.Succeeded);
        Assert.Equal(5, updated.Value!.Exercises[0].TargetSets);

        var removed = await _sut.RemovePlanExerciseAsync(OwnerUserId, planId, lineId);
        Assert.True(removed.Succeeded);
        Assert.Empty(removed.Value!.Exercises);
    }

    [Fact]
    public async Task Delete_plan_removes_it_for_owner()
    {
        var created = await _sut.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "Temp" });
        var planId = created.Value!.Id;

        var deleted = await _sut.DeleteAsync(OwnerUserId, planId);
        Assert.True(deleted.Succeeded);

        var gone = await _sut.GetByIdAsync(OwnerUserId, planId);
        Assert.False(gone.Succeeded);
    }
}
