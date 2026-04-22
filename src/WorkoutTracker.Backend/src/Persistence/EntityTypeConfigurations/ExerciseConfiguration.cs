using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.EntityTypeConfigurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Category)
            .IsRequired();

        builder.Property(e => e.MuscleGroup)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.Name)
            .IsUnique();

        // Navigation properties
        builder.HasMany(e => e.WorkoutPlanExercises)
            .WithOne(wpe => wpe.Exercise)
            .HasForeignKey(wpe => wpe.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.WorkoutSessionExercises)
            .WithOne(wse => wse.Exercise)
            .HasForeignKey(wse => wse.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("exercises");
    }
}
