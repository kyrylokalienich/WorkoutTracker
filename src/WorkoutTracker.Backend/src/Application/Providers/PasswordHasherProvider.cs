using System.Security.Cryptography;
using System.Text;
using WorkoutTracker.Application.Interfaces.Providers;

namespace WorkoutTracker.Application.Providers;

public class PasswordHasherProvider : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public (string Hash, string Salt) HashPassword(string password)
    {
        using var salt = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithm);
        var saltBytes = salt.Salt;
        var key = salt.GetBytes(KeySize);

        var saltString = Convert.ToBase64String(saltBytes);
        var keyString = Convert.ToBase64String(key);

        return (keyString, saltString);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithm);
            var key = pbkdf2.GetBytes(KeySize);
            var keyString = Convert.ToBase64String(key);
            
            return keyString == hash;
        }
        catch
        {
            return false;
        }
    }
}
