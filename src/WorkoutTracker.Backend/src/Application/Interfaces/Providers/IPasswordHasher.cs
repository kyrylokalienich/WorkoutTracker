namespace WorkoutTracker.Application.Interfaces.Providers;

public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password with a generated salt.
    /// </summary>
    /// <param name="password">The plain-text password</param>
    /// <returns>A tuple of (hash, salt)</returns>
    (string Hash, string Salt) HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash and salt.
    /// </summary>
    /// <param name="password">The plain-text password to verify</param>
    /// <param name="hash">The stored password hash</param>
    /// <param name="salt">The stored password salt</param>
    /// <returns>True if password matches; false otherwise</returns>
    bool VerifyPassword(string password, string hash, string salt);
}
