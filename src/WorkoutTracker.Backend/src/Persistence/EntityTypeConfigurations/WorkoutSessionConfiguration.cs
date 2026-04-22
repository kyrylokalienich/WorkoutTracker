using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.EntityTypeConfigurations;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.UserId)
            .IsRequired();

        builder.Property(ws => ws.WorkoutPlanId);

        builder.Property(ws => ws.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ws => ws.ScheduledAtUtc)
            .IsRequired();

        builder.Property(ws => ws.StartedAtUtc);

        builder.Property(ws => ws.CompletedAtUtc);

        builder.Property(ws => ws.Status)
            .IsRequired();

        builder.Property(ws => ws.Comments)
            .HasMaxLength(2000);

        builder.Property(ws => ws.CreatedAtUtc)
            .IsRequired();

        builder.Property(ws => ws.UpdatedAtUtc);

        // Indexes
        builder.HasIndex(ws => ws.UserId);
        builder.HasIndex(ws => new { ws.UserId, ws.ScheduledAtUtc });

        // Navigation properties
        builder.HasMany(ws => ws.WorkoutSessionExercises)
            .WithOne(wse => wse.WorkoutSession)
            .HasForeignKey(wse => wse.WorkoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("workout_sessions");
    }
}
