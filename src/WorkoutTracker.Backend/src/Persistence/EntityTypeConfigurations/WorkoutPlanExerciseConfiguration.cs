using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.EntityTypeConfigurations;

public class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasKey(wpe => wpe.Id);

        builder.Property(wpe => wpe.WorkoutPlanId)
            .IsRequired();

        builder.Property(wpe => wpe.ExerciseId)
            .IsRequired();

        builder.Property(wpe => wpe.TargetSets)
            .IsRequired();

        builder.Property(wpe => wpe.TargetReps)
            .IsRequired();

        builder.Property(wpe => wpe.TargetWeightKg)
            .HasPrecision(8, 2);

        builder.Property(wpe => wpe.OrderIndex)
            .IsRequired();

        // Indexes
        builder.HasIndex(wpe => wpe.WorkoutPlanId);
        builder.HasIndex(wpe => wpe.ExerciseId);

        builder.ToTable("workout_plan_exercises");
    }
}
