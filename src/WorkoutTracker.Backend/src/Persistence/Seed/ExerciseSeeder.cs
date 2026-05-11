using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Persistence.Seed;

public static class ExerciseSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingNames = await dbContext.Exercises
            .AsNoTracking()
            .Select(e => e.Name)
            .ToListAsync(cancellationToken);

        var existing = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        var seeds = GetSeedData();
        var toAdd = seeds.Where(e => !existing.Contains(e.Name)).ToList();

        if (toAdd.Count == 0)
        {
            return;
        }

        await dbContext.Exercises.AddRangeAsync(toAdd, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<Exercise> GetSeedData()
    {
        return new List<Exercise>
        {
            new()
            {
                Name = "Running",
                Description = "Steady-state or interval running.",
                Category = ExerciseCategory.Cardio,
                MuscleGroup = MuscleGroup.FullBody
            },
            new()
            {
                Name = "Cycling",
                Description = "Stationary bike or outdoor cycling.",
                Category = ExerciseCategory.Cardio,
                MuscleGroup = MuscleGroup.Legs
            },
            new()
            {
                Name = "Jump Rope",
                Description = "Jump rope cardio conditioning.",
                Category = ExerciseCategory.Cardio,
                MuscleGroup = MuscleGroup.FullBody
            },
            new()
            {
                Name = "Bench Press",
                Description = "Barbell bench press.",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Chest
            },
            new()
            {
                Name = "Push-Up",
                Description = "Bodyweight push-up.",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Chest
            },
            new()
            {
                Name = "Deadlift",
                Description = "Barbell deadlift.",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Back
            },
            new()
            {
                Name = "Pull-Up",
                Description = "Bodyweight pull-up.",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Back
            },
            new()
            {
                Name = "Squat",
                Description = "Barbell back squat.",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Legs
            },
            new()
            {
                Name = "Lunges",
                Description = "Forward or walking lunges.",
                Category = ExerciseCategory.Strength,
                MuscleGroup = MuscleGroup.Legs
            },
            new()
            {
                Name = "Hamstring Stretch",
                Description = "Static hamstring stretch.",
                Category = ExerciseCategory.Flexibility,
                MuscleGroup = MuscleGroup.Hamstrings
            },
            new()
            {
                Name = "Hip Flexor Stretch",
                Description = "Static hip flexor stretch.",
                Category = ExerciseCategory.Flexibility,
                MuscleGroup = MuscleGroup.Legs
            }
        };
    }
}

