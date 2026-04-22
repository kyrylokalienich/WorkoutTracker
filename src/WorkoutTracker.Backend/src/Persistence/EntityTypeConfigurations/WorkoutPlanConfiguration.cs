using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.EntityTypeConfigurations;

public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasKey(wp => wp.Id);

        builder.Property(wp => wp.UserId)
            .IsRequired();

        builder.Property(wp => wp.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(wp => wp.Description)
            .HasMaxLength(1000);

        builder.Property(wp => wp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(wp => wp.CreatedAtUtc)
            .IsRequired();

        builder.Property(wp => wp.UpdatedAtUtc);

        // Indexes
        builder.HasIndex(wp => wp.UserId);

        // Navigation properties
        builder.HasMany(wp => wp.WorkoutPlanExercises)
            .WithOne(wpe => wpe.WorkoutPlan)
            .HasForeignKey(wpe => wpe.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(wp => wp.WorkoutSessions)
            .WithOne(ws => ws.WorkoutPlan)
            .HasForeignKey(ws => ws.WorkoutPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("workout_plans");
    }
}
