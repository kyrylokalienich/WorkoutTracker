using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReportQueries_UserCompletedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_UserId_CompletedAtUtc",
                table: "workout_sessions",
                columns: new[] { "UserId", "CompletedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workout_sessions_UserId_CompletedAtUtc",
                table: "workout_sessions");
        }
    }
}
