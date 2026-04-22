using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.EntityTypeConfigurations;

public class WorkoutSessionExerciseConfiguration : IEntityTypeConfiguration<WorkoutSessionExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutSessionExercise> builder)
    {
        builder.HasKey(wse => wse.Id);

        builder.Property(wse => wse.WorkoutSessionId)
            .IsRequired();

        builder.Property(wse => wse.ExerciseId)
            .IsRequired();

        builder.Property(wse => wse.PlannedSets)
            .IsRequired();

        builder.Property(wse => wse.PlannedReps)
            .IsRequired();

        builder.Property(wse => wse.PlannedWeightKg)
            .HasPrecision(8, 2);

        builder.Property(wse => wse.ActualSets);

        builder.Property(wse => wse.ActualReps);

        builder.Property(wse => wse.ActualWeightKg)
            .HasPrecision(8, 2);

        builder.Property(wse => wse.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(wse => wse.WorkoutSessionId);
        builder.HasIndex(wse => wse.ExerciseId);

        builder.ToTable("workout_session_exercises");
    }
}
