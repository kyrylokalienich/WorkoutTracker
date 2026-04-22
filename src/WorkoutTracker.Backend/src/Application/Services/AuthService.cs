using WorkoutTracker.Application.Interfaces.Providers;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.UnitOfWork;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<(bool Success, string? Message)> SignUpAsync(string email, string username, string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return (false, "Email, username, and password are required.");
        }

        // Check if email already exists
        if (await EmailExistsAsync(email))
        {
            return (false, "Email is already in use.");
        }

        // Check if username already exists
        if (await UsernameExistsAsync(username))
        {
            return (false, "Username is already in use.");
        }

        // Hash password
        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Create user
        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = UserRole.User,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, int? UserId, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt)>
        SignInAsync(string email, string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, null, null, null, null);
        }

        // Find user by email
        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            return (false, null, null, null, null);
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            return (false, null, null, null, null);
        }

        // Generate tokens
        var accessToken = _jwtProvider.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        // Hash refresh token before storing
        var (refreshTokenHash, refreshTokenSalt) = _passwordHasher.HashPassword(refreshToken);

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        var expiresAt = _dateTimeProvider.UtcNow.AddMinutes(15);
        return (true, user.Id, accessToken, refreshToken, expiresAt);
    }

    public async Task<(bool Success, string? AccessToken, string? NewRefreshToken, DateTime? ExpiresAt)>
        RefreshTokenAsync(int userId, string oldRefreshToken)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(oldRefreshToken))
        {
            return (false, null, null, null);
        }

        // Find user
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return (false, null, null, null);
        }

        // Find and validate refresh token
        var refreshTokenRepository = _unitOfWork.Repository<RefreshToken>();
        var refreshTokens = await refreshTokenRepository.GetAllAsync();
        var refreshTokenEntity = refreshTokens
            .FirstOrDefault(rt => rt.UserId == userId && !rt.IsRevoked &&
                                   rt.ExpiresAtUtc > _dateTimeProvider.UtcNow);

        if (refreshTokenEntity == null)
        {
            return (false, null, null, null);
        }

        // Verify the refresh token
        if (!_passwordHasher.VerifyPassword(oldRefreshToken, refreshTokenEntity.TokenHash,
                refreshTokenEntity.TokenHash))
        {
            return (false, null, null, null);
        }

        // Revoke old token
        refreshTokenEntity.IsRevoked = true;
        await refreshTokenRepository.UpdateAsync(refreshTokenEntity);

        // Generate new access token and refresh token
        var newAccessToken = _jwtProvider.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        // Hash and store new refresh token
        var (newRefreshTokenHash, newRefreshTokenSalt) = _passwordHasher.HashPassword(newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        await refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        var expiresAt = _dateTimeProvider.UtcNow.AddMinutes(15);
        return (true, newAccessToken, newRefreshToken, expiresAt);
    }

    public async Task<bool> LogoutAsync(int userId, string refreshToken)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        // Find and revoke refresh token
        var refreshTokenRepository = _unitOfWork.Repository<RefreshToken>();
        var refreshTokens = await refreshTokenRepository.GetAllAsync();
        var refreshTokenEntity = refreshTokens.FirstOrDefault(rt => rt.UserId == userId && !rt.IsRevoked);

        if (refreshTokenEntity == null)
        {
            return false;
        }

        // Verify the refresh token
        if (!_passwordHasher.VerifyPassword(refreshToken, refreshTokenEntity.TokenHash,
                refreshTokenEntity.TokenHash))
        {
            return false;
        }

        // Revoke token
        refreshTokenEntity.IsRevoked = true;
        await refreshTokenRepository.UpdateAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.GetAllAsync();
        return users.Any(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.GetAllAsync();
        return users.Any(u => u.Username == username);
    }
}
