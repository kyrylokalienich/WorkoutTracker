using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Application.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Persistence;
using WorkoutTracker.Persistence.UnitOfWork;

namespace WorkoutTracker.Tests.Services;

public sealed class WorkoutSessionServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly WorkoutPlanService _planService;
    private readonly WorkoutSessionService _sut;

    public WorkoutSessionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"workout_sessions_tests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _planService = new WorkoutPlanService(_unitOfWork);
        _sut = new WorkoutSessionService(_unitOfWork);
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
            Name = "Squat",
            Category = ExerciseCategory.Strength,
            MuscleGroup = MuscleGroup.Legs
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
    public async Task Schedule_from_plan_copies_planned_targets()
    {
        var plan = await _planService.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "Leg day" });
        var planId = plan.Value!.Id;
        await _planService.AddExerciseAsync(
            OwnerUserId,
            planId,
            new AddPlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 4,
                TargetReps = 8,
                TargetWeightKg = 100m,
                OrderIndex = 0
            });

        var scheduled = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
        var created = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = planId,
                Title = "Morning legs",
                ScheduledAtUtc = scheduled
            });

        Assert.True(created.Succeeded);
        Assert.Equal(planId, created.Value!.WorkoutPlanId);
        Assert.Single(created.Value.Exercises);
        var ex = created.Value.Exercises[0];
        Assert.Equal(ExerciseId, ex.ExerciseId);
        Assert.Equal(4, ex.PlannedSets);
        Assert.Equal(8, ex.PlannedReps);
        Assert.Equal(100m, ex.PlannedWeightKg);
    }

    [Fact]
    public async Task Schedule_with_unknown_plan_returns_not_found()
    {
        var r = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = 999_999,
                Title = "X",
                ScheduledAtUtc = DateTime.UtcNow
            });
        Assert.False(r.Succeeded);
        Assert.Equal("not_found", r.FailureCode);
    }

    [Fact]
    public async Task List_default_sort_is_scheduledAt_asc_with_pagination()
    {
        var t0 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "C", ScheduledAtUtc = t0.AddDays(2) });
        await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "A", ScheduledAtUtc = t0 });
        await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "B", ScheduledAtUtc = t0.AddDays(1) });

        var page1 = await _sut.ListAsync(OwnerUserId, null, null, null, page: 1, pageSize: 2);
        Assert.True(page1.Succeeded);
        Assert.Equal(3, page1.Value!.TotalCount);
        Assert.Equal(2, page1.Value.Items.Count);
        Assert.Equal("A", page1.Value.Items[0].Title);
        Assert.Equal("B", page1.Value.Items[1].Title);

        var page2 = await _sut.ListAsync(OwnerUserId, null, null, null, page: 2, pageSize: 2);
        Assert.Single(page2.Value!.Items);
        Assert.Equal("C", page2.Value.Items[0].Title);
    }

    [Fact]
    public async Task List_filters_by_status_and_date_range()
    {
        var day = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);
        var s1 = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "In range", ScheduledAtUtc = day });
        await _sut.UpdateAsync(
            OwnerUserId,
            s1.Value!.Id,
            new UpdateWorkoutSessionRequest
            {
                Title = "In range",
                ScheduledAtUtc = day,
                Status = WorkoutStatus.Skipped
            });

        await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                Title = "Later",
                ScheduledAtUtc = day.AddDays(10)
            });

        var plannedOnly = await _sut.ListAsync(
            OwnerUserId,
            WorkoutStatus.Planned,
            fromUtc: day.AddDays(1),
            toUtc: day.AddDays(20),
            page: 1,
            pageSize: 10);
        Assert.True(plannedOnly.Succeeded);
        Assert.Single(plannedOnly.Value!.Items);
        Assert.Equal("Later", plannedOnly.Value.Items[0].Title);

        var skipped = await _sut.ListAsync(
            OwnerUserId,
            WorkoutStatus.Skipped,
            fromUtc: day.AddDays(-1),
            toUtc: day.AddDays(1),
            page: 1,
            pageSize: 10);
        Assert.Single(skipped.Value!.Items);
        Assert.Equal(WorkoutStatus.Skipped, skipped.Value.Items[0].Status);
    }

    [Fact]
    public async Task GetById_denies_other_user()
    {
        var created = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "Mine", ScheduledAtUtc = DateTime.UtcNow });
        var foreign = await _sut.GetByIdAsync(OtherUserId, created.Value!.Id);
        Assert.False(foreign.Succeeded);
        Assert.Equal("not_found", foreign.FailureCode);
    }

    [Fact]
    public async Task Update_allows_planned_to_inprogress_and_skipped_only()
    {
        var created = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "W", ScheduledAtUtc = DateTime.UtcNow });

        var start = await _sut.UpdateAsync(
            OwnerUserId,
            created.Value!.Id,
            new UpdateWorkoutSessionRequest
            {
                Title = "W",
                ScheduledAtUtc = created.Value.ScheduledAtUtc,
                Status = WorkoutStatus.InProgress
            });
        Assert.True(start.Succeeded);
        Assert.Equal(WorkoutStatus.InProgress, start.Value!.Status);
        Assert.NotNull(start.Value.StartedAtUtc);

        var bad = await _sut.UpdateAsync(
            OwnerUserId,
            created.Value.Id,
            new UpdateWorkoutSessionRequest
            {
                Title = "W",
                ScheduledAtUtc = created.Value.ScheduledAtUtc,
                Status = WorkoutStatus.Skipped
            });
        Assert.False(bad.Succeeded);
        Assert.Equal("invalid_transition", bad.FailureCode);
    }

    [Fact]
    public async Task Update_blocks_when_completed()
    {
        var created = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "Done", ScheduledAtUtc = DateTime.UtcNow });
        await _sut.CompleteAsync(
            OwnerUserId,
            created.Value!.Id,
            new CompleteWorkoutSessionRequest { Exercises = new List<CompleteSessionExerciseRequest>() });

        var upd = await _sut.UpdateAsync(
            OwnerUserId,
            created.Value.Id,
            new UpdateWorkoutSessionRequest
            {
                Title = "Nope",
                ScheduledAtUtc = DateTime.UtcNow
            });
        Assert.False(upd.Succeeded);
        Assert.Equal("invalid_state", upd.FailureCode);
    }

    [Fact]
    public async Task Complete_persists_actuals_from_planned_or_in_progress()
    {
        var plan = await _planService.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "P" });
        await _planService.AddExerciseAsync(
            OwnerUserId,
            plan.Value!.Id,
            new AddPlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 3,
                TargetReps = 5,
                OrderIndex = 0
            });

        var session = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "S",
                ScheduledAtUtc = DateTime.UtcNow
            });
        var lineId = session.Value!.Exercises[0].Id;

        var done = await _sut.CompleteAsync(
            OwnerUserId,
            session.Value.Id,
            new CompleteWorkoutSessionRequest
            {
                Exercises = new List<CompleteSessionExerciseRequest>
                {
                    new()
                    {
                        SessionExerciseId = lineId,
                        ActualSets = 3,
                        ActualReps = 5,
                        ActualWeightKg = 90m,
                        Notes = " Felt good "
                    }
                }
            });

        Assert.True(done.Succeeded);
        Assert.Equal(WorkoutStatus.Completed, done.Value!.Status);
        Assert.NotNull(done.Value.CompletedAtUtc);
        var ex = Assert.Single(done.Value.Exercises);
        Assert.Equal(3, ex.ActualSets);
        Assert.Equal(5, ex.ActualReps);
        Assert.Equal(90m, ex.ActualWeightKg);
        Assert.Equal("Felt good", ex.Notes);
    }

    [Fact]
    public async Task Complete_rejects_mismatched_exercise_payload()
    {
        var plan = await _planService.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "P2" });
        await _planService.AddExerciseAsync(
            OwnerUserId,
            plan.Value!.Id,
            new AddPlanExerciseRequest
            {
                ExerciseId = ExerciseId,
                TargetSets = 1,
                TargetReps = 1,
                OrderIndex = 0
            });

        var session = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "S2",
                ScheduledAtUtc = DateTime.UtcNow
            });

        var bad = await _sut.CompleteAsync(
            OwnerUserId,
            session.Value!.Id,
            new CompleteWorkoutSessionRequest { Exercises = new List<CompleteSessionExerciseRequest>() });
        Assert.False(bad.Succeeded);
        Assert.Equal("validation_failed", bad.FailureCode);
    }

    [Fact]
    public async Task Delete_removes_session_for_owner()
    {
        var created = await _sut.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest { Title = "Del", ScheduledAtUtc = DateTime.UtcNow });
        var del = await _sut.DeleteAsync(OwnerUserId, created.Value!.Id);
        Assert.True(del.Succeeded);

        var gone = await _sut.GetByIdAsync(OwnerUserId, created.Value.Id);
        Assert.False(gone.Succeeded);
    }
}
