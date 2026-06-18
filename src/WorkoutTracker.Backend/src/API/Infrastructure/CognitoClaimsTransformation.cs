using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Persistence;

namespace WorkoutTracker.API.Infrastructure;

/// <summary>
/// Bridges the Cognito identity to the local domain: on the first request from a
/// Cognito user, a local <see cref="User"/> row is created (just-in-time provisioning)
/// and linked by the Cognito <c>sub</c>. An <c>app:userId</c> claim carrying the local
/// integer id is added so existing controllers keep working unchanged.
/// </summary>
public class CognitoClaimsTransformation : IClaimsTransformation
{
    public const string UserIdClaim = "app:userId";

    private readonly AppDbContext _db;

    public CognitoClaimsTransformation(AppDbContext db) => _db = db;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Runs on every request; do nothing for anonymous calls or if already mapped.
        if (principal.Identity is not { IsAuthenticated: true } ||
            principal.HasClaim(c => c.Type == UserIdClaim))
        {
            return principal;
        }

        var sub = principal.FindFirstValue("sub")
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sub))
            return principal;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.CognitoSub == sub);
        if (user is null)
        {
            var email = principal.FindFirstValue("email") ?? $"{sub}@cognito.local";
            var username = principal.FindFirstValue("name") ?? email;

            user = new User
            {
                CognitoSub = sub,
                Email = email,
                Username = username,
                PasswordHash = string.Empty,   // unused — Cognito owns credentials
                PasswordSalt = string.Empty,
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(UserIdClaim, user.Id.ToString()));
        return principal;
    }
}
