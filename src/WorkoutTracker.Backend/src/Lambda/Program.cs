using Amazon.Lambda.AspNetCoreServer.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Runs this minimal API as a Lambda behind an API Gateway HTTP API (or Function URL)
// in AWS, and as a normal Kestrel app when run locally — same code, both targets.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.MapGet("/", () => "WorkoutTracker Lambda is alive");

// Estimate one-rep max (1RM) from a working set. Pure compute — no DB, no VPC.
app.MapPost("/one-rep-max", (OneRepMaxRequest req) =>
{
    if (req.Weight <= 0 || req.Reps < 1 || req.Reps > 30)
        return Results.BadRequest(new { error = "weight must be > 0 and reps must be between 1 and 30" });

    // Epley:   1RM = w * (1 + reps/30)
    // Brzycki: 1RM = w * 36 / (37 - reps)
    var epley = req.Weight * (1 + req.Reps / 30.0);
    var brzycki = req.Weight * 36.0 / (37 - req.Reps);

    return Results.Ok(new OneRepMaxResponse(
        req.Weight,
        req.Reps,
        Math.Round(epley, 1),
        Math.Round(brzycki, 1)));
});

app.Run();

record OneRepMaxRequest(double Weight, int Reps);
record OneRepMaxResponse(double Weight, int Reps, double Epley, double Brzycki);
