using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WorkoutTracker.API.Infrastructure;

/// <summary>
/// Injects concrete request/response examples into the Swagger document for the five key user flows.
/// </summary>
public sealed class SwaggerExamplesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.MethodInfo.DeclaringType?.Name;
        var action = context.MethodInfo.Name;

        switch ((controller, action))
        {
            case ("AuthController", "SignUp"):
                SetRequestExample(operation, new OpenApiObject
                {
                    ["email"] = new OpenApiString("john.doe@example.com"),
                    ["username"] = new OpenApiString("johndoe"),
                    ["password"] = new OpenApiString("SecurePass123!"),
                    ["confirmPassword"] = new OpenApiString("SecurePass123!")
                });
                break;

            case ("AuthController", "SignIn"):
                SetRequestExample(operation, new OpenApiObject
                {
                    ["email"] = new OpenApiString("john.doe@example.com"),
                    ["password"] = new OpenApiString("SecurePass123!")
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(1),
                    ["email"] = new OpenApiString("john.doe@example.com"),
                    ["username"] = new OpenApiString("johndoe"),
                    ["accessToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIn0.example"),
                    ["refreshToken"] = new OpenApiString("dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4"),
                    ["expiresAt"] = new OpenApiString("2026-06-06T15:00:00Z")
                });
                break;

            case ("WorkoutPlansController", "CreateWorkoutPlan"):
                SetRequestExample(operation, new OpenApiObject
                {
                    ["name"] = new OpenApiString("Upper Body Power"),
                    ["description"] = new OpenApiString("Chest, back, and shoulders compound lifts"),
                    ["isActive"] = new OpenApiBoolean(true)
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(1),
                    ["name"] = new OpenApiString("Upper Body Power"),
                    ["description"] = new OpenApiString("Chest, back, and shoulders compound lifts"),
                    ["isActive"] = new OpenApiBoolean(true),
                    ["createdAtUtc"] = new OpenApiString("2026-06-06T10:00:00Z"),
                    ["updatedAtUtc"] = new OpenApiNull(),
                    ["exercises"] = new OpenApiArray()
                });
                break;

            case ("WorkoutSessionsController", "ScheduleWorkoutSession"):
                SetRequestExample(operation, new OpenApiObject
                {
                    ["workoutPlanId"] = new OpenApiInteger(1),
                    ["title"] = new OpenApiString("Monday Upper Body"),
                    ["scheduledAtUtc"] = new OpenApiString("2026-06-09T07:00:00Z")
                });
                SetResponseExample(operation, "201", new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(10),
                    ["workoutPlanId"] = new OpenApiInteger(1),
                    ["title"] = new OpenApiString("Monday Upper Body"),
                    ["scheduledAtUtc"] = new OpenApiString("2026-06-09T07:00:00Z"),
                    ["startedAtUtc"] = new OpenApiNull(),
                    ["completedAtUtc"] = new OpenApiNull(),
                    ["status"] = new OpenApiString("Planned"),
                    ["comments"] = new OpenApiNull(),
                    ["createdAtUtc"] = new OpenApiString("2026-06-06T10:00:00Z"),
                    ["updatedAtUtc"] = new OpenApiNull(),
                    ["exercises"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["id"] = new OpenApiInteger(1),
                            ["exerciseId"] = new OpenApiInteger(3),
                            ["exerciseName"] = new OpenApiString("Bench Press"),
                            ["plannedSets"] = new OpenApiInteger(4),
                            ["plannedReps"] = new OpenApiInteger(8),
                            ["plannedWeightKg"] = new OpenApiDouble(80.0),
                            ["actualSets"] = new OpenApiNull(),
                            ["actualReps"] = new OpenApiNull(),
                            ["actualWeightKg"] = new OpenApiNull(),
                            ["notes"] = new OpenApiNull()
                        }
                    }
                });
                break;

            case ("WorkoutSessionsController", "CompleteWorkoutSession"):
                SetRequestExample(operation, new OpenApiObject
                {
                    ["comments"] = new OpenApiString("Felt strong today"),
                    ["exercises"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["sessionExerciseId"] = new OpenApiInteger(1),
                            ["actualSets"] = new OpenApiInteger(4),
                            ["actualReps"] = new OpenApiInteger(8),
                            ["actualWeightKg"] = new OpenApiDouble(82.5),
                            ["notes"] = new OpenApiString("Increased weight by 2.5 kg")
                        }
                    }
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(10),
                    ["workoutPlanId"] = new OpenApiInteger(1),
                    ["title"] = new OpenApiString("Monday Upper Body"),
                    ["scheduledAtUtc"] = new OpenApiString("2026-06-09T07:00:00Z"),
                    ["startedAtUtc"] = new OpenApiString("2026-06-09T07:02:00Z"),
                    ["completedAtUtc"] = new OpenApiString("2026-06-09T08:05:00Z"),
                    ["status"] = new OpenApiString("Completed"),
                    ["comments"] = new OpenApiString("Felt strong today"),
                    ["createdAtUtc"] = new OpenApiString("2026-06-06T10:00:00Z"),
                    ["updatedAtUtc"] = new OpenApiString("2026-06-09T08:05:00Z"),
                    ["exercises"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["id"] = new OpenApiInteger(1),
                            ["exerciseId"] = new OpenApiInteger(3),
                            ["exerciseName"] = new OpenApiString("Bench Press"),
                            ["plannedSets"] = new OpenApiInteger(4),
                            ["plannedReps"] = new OpenApiInteger(8),
                            ["plannedWeightKg"] = new OpenApiDouble(80.0),
                            ["actualSets"] = new OpenApiInteger(4),
                            ["actualReps"] = new OpenApiInteger(8),
                            ["actualWeightKg"] = new OpenApiDouble(82.5),
                            ["notes"] = new OpenApiString("Increased weight by 2.5 kg")
                        }
                    }
                });
                break;

            case ("ReportsController", "GetProgress"):
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["completedWorkoutCount"] = new OpenApiInteger(12),
                    ["totalVolumeKg"] = new OpenApiDouble(45600.0),
                    ["averageVolumeKgPerWorkout"] = new OpenApiDouble(3800.0),
                    ["scheduledCompletedCount"] = new OpenApiInteger(12),
                    ["scheduledSkippedCount"] = new OpenApiInteger(2),
                    ["completionRate"] = new OpenApiDouble(0.857)
                });
                break;
        }
    }

    private static void SetRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        if (operation.RequestBody?.Content?.ContainsKey("application/json") == true)
            operation.RequestBody.Content["application/json"].Example = example;
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (operation.Responses.TryGetValue(statusCode, out var response) &&
            response.Content?.ContainsKey("application/json") == true)
            response.Content["application/json"].Example = example;
    }
}
