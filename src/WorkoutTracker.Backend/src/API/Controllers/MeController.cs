using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence;

namespace WorkoutTracker.API.Controllers;

/// <summary>Current user profile. Auth itself is handled by Cognito Hosted UI.</summary>
[Route("api/auth")]
[Produces("application/json")]
public class MeController : BaseController
{
    private readonly AppDbContext _db;

    public MeController(AppDbContext db) => _db = db;

    /// <summary>Returns the local profile of the authenticated (JIT-provisioned) Cognito user.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = GetCurrentUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Username,
            Role = user.Role.ToString()
        });
    }
}
