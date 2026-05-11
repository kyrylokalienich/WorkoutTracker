using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UniquePlanExercisePerPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_workout_plan_exercises_WorkoutPlanId_ExerciseId",
                table: "workout_plan_exercises",
                columns: new[] { "WorkoutPlanId", "ExerciseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workout_plan_exercises_WorkoutPlanId_ExerciseId",
                table: "workout_plan_exercises");
        }
    }
}
