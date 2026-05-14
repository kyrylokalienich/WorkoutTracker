using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Application.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Persistence;
using WorkoutTracker.Persistence.UnitOfWork;

namespace WorkoutTracker.Tests.Services;

public sealed class ReportServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly WorkoutPlanService _planService;
    private readonly WorkoutSessionService _sessionService;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"report_tests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _planService = new WorkoutPlanService(_unitOfWork);
        _sessionService = new WorkoutSessionService(_unitOfWork);
        _sut = new ReportService(_unitOfWork);
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
        _context.Exercises.AddRange(
            new Exercise
            {
                Name = "Squat",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Legs
            },
            new Exercise
            {
                Name = "Row",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Back
            });
        _context.SaveChanges();
    }

    private int OwnerUserId => _context.Users.Single(u => u.Username == "owner").Id;
    private int LegsExerciseId => _context.Exercises.Single(e => e.Name == "Squat").Id;
    private int BackExerciseId => _context.Exercises.Single(e => e.Name == "Row").Id;

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Reports_require_from_and_to()
    {
        var r = await _sut.GetWorkoutHistoryAsync(OwnerUserId, null, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.False(r.Succeeded);
        Assert.Equal("validation_failed", r.FailureCode);
    }

    [Fact]
    public async Task Reports_reject_inverted_range()
    {
        var from = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var r = await _sut.GetProgressAsync(OwnerUserId, from, to);
        Assert.False(r.Succeeded);
        Assert.Equal("validation_failed", r.FailureCode);
    }

    [Fact]
    public async Task Reports_reject_range_over_max_days()
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddDays(732);
        var r = await _sut.GetMuscleGroupsAsync(OwnerUserId, from, to);
        Assert.False(r.Succeeded);
        Assert.Equal("validation_failed", r.FailureCode);
    }

    [Fact]
    public async Task Workout_history_lists_completed_only_in_completedAt_range_with_volume()
    {
        var plan = await _planService.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "P" });
        await _planService.AddExerciseAsync(
            OwnerUserId,
            plan.Value!.Id,
            new AddPlanExerciseRequest
            {
                ExerciseId = LegsExerciseId,
                TargetSets = 2,
                TargetReps = 10,
                TargetWeightKg = 40m,
                OrderIndex = 0
            });

        var s1 = await _sessionService.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "In window",
                ScheduledAtUtc = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc)
            });
        var line1 = s1.Value!.Exercises[0].Id;
        await _sessionService.CompleteAsync(
            OwnerUserId,
            s1.Value.Id,
            new CompleteWorkoutSessionRequest
            {
                Exercises = new List<CompleteSessionExerciseRequest>
                {
                    new()
                    {
                        SessionExerciseId = line1,
                        ActualSets = 2,
                        ActualReps = 10,
                        ActualWeightKg = 50m
                    }
                }
            });

        var completedMid = new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);
        await StampCompletedAt(s1.Value.Id, completedMid);

        var s2 = await _sessionService.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "Outside window",
                ScheduledAtUtc = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc)
            });
        var line2 = s2.Value!.Exercises[0].Id;
        await _sessionService.CompleteAsync(
            OwnerUserId,
            s2.Value.Id,
            new CompleteWorkoutSessionRequest
            {
                Exercises = new List<CompleteSessionExerciseRequest>
                {
                    new() { SessionExerciseId = line2, ActualSets = 1, ActualReps = 1, ActualWeightKg = 1m }
                }
            });
        await StampCompletedAt(s2.Value.Id, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));

        var from = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);
        var history = await _sut.GetWorkoutHistoryAsync(OwnerUserId, from, to);
        Assert.True(history.Succeeded);
        var item = Assert.Single(history.Value!.Items);
        Assert.Equal("In window", item.Title);
        Assert.Equal(completedMid, item.CompletedAtUtc);
        Assert.Equal(1, item.ExerciseCount);
        Assert.Equal(1000m, item.TotalVolumeKg);
    }

    [Fact]
    public async Task Progress_aggregates_volume_and_completion_rate()
    {
        var plan = await _planService.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "Mix" });
        await _planService.AddExerciseAsync(
            OwnerUserId,
            plan.Value!.Id,
            new AddPlanExerciseRequest
            {
                ExerciseId = LegsExerciseId,
                TargetSets = 1,
                TargetReps = 1,
                TargetWeightKg = 10m,
                OrderIndex = 0
            });

        var march = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

        var completedSession = await _sessionService.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "Done",
                ScheduledAtUtc = march
            });
        var lineA = completedSession.Value!.Exercises[0].Id;
        await _sessionService.CompleteAsync(
            OwnerUserId,
            completedSession.Value.Id,
            new CompleteWorkoutSessionRequest
            {
                Exercises = new List<CompleteSessionExerciseRequest>
                {
                    new() { SessionExerciseId = lineA, ActualSets = 1, ActualReps = 10, ActualWeightKg = 20m }
                }
            });
        await StampCompletedAt(completedSession.Value.Id, march.AddDays(1));

        var skipped = await _sessionService.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "Skip me",
                ScheduledAtUtc = march.AddDays(2)
            });
        await _sessionService.UpdateAsync(
            OwnerUserId,
            skipped.Value!.Id,
            new UpdateWorkoutSessionRequest
            {
                Title = "Skip me",
                ScheduledAtUtc = march.AddDays(2),
                Status = WorkoutStatus.Skipped
            });

        var from = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);

        var progress = await _sut.GetProgressAsync(OwnerUserId, from, to);
        Assert.True(progress.Succeeded);
        Assert.Equal(1, progress.Value!.CompletedWorkoutCount);
        Assert.Equal(200m, progress.Value.TotalVolumeKg);
        Assert.Equal(200d, progress.Value.AverageVolumeKgPerWorkout);
        Assert.Equal(1, progress.Value.ScheduledCompletedCount);
        Assert.Equal(1, progress.Value.ScheduledSkippedCount);
        Assert.Equal(0.5, progress.Value.CompletionRate);

        var fromVol = new DateTime(2026, 3, 11, 0, 0, 0, DateTimeKind.Utc);
        var toVol = new DateTime(2026, 3, 12, 23, 59, 59, DateTimeKind.Utc);
        var progressVolWindow = await _sut.GetProgressAsync(OwnerUserId, fromVol, toVol);
        Assert.True(progressVolWindow.Succeeded);
        Assert.Equal(1, progressVolWindow.Value!.CompletedWorkoutCount);
        Assert.Equal(200m, progressVolWindow.Value.TotalVolumeKg);
    }

    [Fact]
    public async Task Muscle_groups_split_volume_by_exercise_muscle()
    {
        var plan = await _planService.CreateAsync(OwnerUserId, new CreateWorkoutPlanRequest { Name = "Full" });
        await _planService.AddExerciseAsync(
            OwnerUserId,
            plan.Value!.Id,
            new AddPlanExerciseRequest
            {
                ExerciseId = LegsExerciseId,
                TargetSets = 1,
                TargetReps = 1,
                TargetWeightKg = 1m,
                OrderIndex = 0
            });
        await _planService.AddExerciseAsync(
            OwnerUserId,
            plan.Value.Id,
            new AddPlanExerciseRequest
            {
                ExerciseId = BackExerciseId,
                TargetSets = 1,
                TargetReps = 1,
                TargetWeightKg = 1m,
                OrderIndex = 1
            });

        var day = new DateTime(2026, 4, 5, 8, 0, 0, DateTimeKind.Utc);
        var session = await _sessionService.ScheduleAsync(
            OwnerUserId,
            new ScheduleWorkoutSessionRequest
            {
                WorkoutPlanId = plan.Value.Id,
                Title = "Double",
                ScheduledAtUtc = day
            });
        var legsLine = session.Value!.Exercises.Single(e => e.ExerciseId == LegsExerciseId).Id;
        var backLine = session.Value.Exercises.Single(e => e.ExerciseId == BackExerciseId).Id;
        await _sessionService.CompleteAsync(
            OwnerUserId,
            session.Value.Id,
            new CompleteWorkoutSessionRequest
            {
                Exercises = new List<CompleteSessionExerciseRequest>
                {
                    new() { SessionExerciseId = legsLine, ActualSets = 2, ActualReps = 5, ActualWeightKg = 10m },
                    new() { SessionExerciseId = backLine, ActualSets = 1, ActualReps = 8, ActualWeightKg = 25m }
                }
            });
        await StampCompletedAt(session.Value.Id, day.AddHours(2));

        var from = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc);
        var mg = await _sut.GetMuscleGroupsAsync(OwnerUserId, from, to);
        Assert.True(mg.Succeeded);
        Assert.Equal(2, mg.Value!.Items.Count);

        var legs = mg.Value.Items.Single(i => i.MuscleGroup == MuscleGroup.Legs);
        Assert.Equal(100m, legs.TotalVolumeKg);
        Assert.Equal(1, legs.SessionExerciseLineCount);

        var back = mg.Value.Items.Single(i => i.MuscleGroup == MuscleGroup.Back);
        Assert.Equal(200m, back.TotalVolumeKg);
        Assert.Equal(1, back.SessionExerciseLineCount);
    }

    private async Task StampCompletedAt(int sessionId, DateTime completedAtUtc)
    {
        var entity = await _context.WorkoutSessions.SingleAsync(s => s.Id == sessionId);
        entity.CompletedAtUtc = completedAtUtc;
        entity.UpdatedAtUtc = completedAtUtc;
        await _context.SaveChangesAsync();
    }
}
