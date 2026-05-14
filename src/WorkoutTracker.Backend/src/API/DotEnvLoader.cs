namespace WorkoutTracker.API;

/// <summary>
/// Loads the first <c>.env</c> file found by walking up from <see cref="Directory.GetCurrentDirectory" />
/// and by checking <c>src/API/.env</c> at each level (so running from repo root still finds the API-local file).
/// </summary>
internal static class DotEnvLoader
{
    public static void Load()
    {
        foreach (var path in GetCandidatePaths())
        {
            if (!File.Exists(path))
            {
                continue;
            }

            DotNetEnv.Env.Load(path);
            return;
        }
    }

    private static IEnumerable<string> GetCandidatePaths()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; i < 12 && dir != null; i++, dir = dir.Parent!)
        {
            foreach (var relative in new[] { ".env", Path.Combine("src", "API", ".env") })
            {
                var full = Path.GetFullPath(Path.Combine(dir.FullName, relative));
                if (seen.Add(full))
                {
                    yield return full;
                }
            }
        }
    }
}
