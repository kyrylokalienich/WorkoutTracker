using Microsoft.Extensions.Logging;
using Moq;
using WorkoutTracker.Application.Interfaces.Providers;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.UnitOfWork;
using WorkoutTracker.Application.Services;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IPasswordHasher> _mockPasswordHasher = new();
    private readonly Mock<IJwtProvider> _mockJwtProvider = new();
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new();
    private readonly Mock<ILogger<AuthService>> _mockLogger = new();
    private readonly Mock<IRepository<User, int>> _mockUserRepo = new();
    private readonly Mock<IRepository<RefreshToken, int>> _mockRefreshTokenRepo = new();

    private readonly AuthService _sut;
    private readonly DateTime _now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public AuthServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Repository<User>()).Returns(_mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.Repository<RefreshToken>()).Returns(_mockRefreshTokenRepo.Object);
        _mockDateTimeProvider.Setup(d => d.UtcNow).Returns(_now);

        _sut = new AuthService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockJwtProvider.Object,
            _mockDateTimeProvider.Object,
            _mockLogger.Object);
    }

    // --- SignUpAsync ---

    [Fact]
    public async Task SignUpAsync_NewUser_ReturnsSuccess()
    {
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<User>());
        _mockPasswordHasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns(("hash", "salt"));
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var (success, message) = await _sut.SignUpAsync("new@example.com", "newuser", "password123");

        Assert.True(success);
        Assert.Null(message);
    }

    [Fact]
    public async Task SignUpAsync_DuplicateEmail_ReturnsFalse()
    {
        var existing = new User { Id = 1, Email = "taken@example.com", Username = "someuser" };
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { existing });

        var (success, message) = await _sut.SignUpAsync("taken@example.com", "newuser", "password123");

        Assert.False(success);
        Assert.Contains("Email", message);
    }

    [Fact]
    public async Task SignUpAsync_DuplicateUsername_ReturnsFalse()
    {
        var existing = new User { Id = 1, Email = "other@example.com", Username = "takenuser" };
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { existing });

        var (success, message) = await _sut.SignUpAsync("new@example.com", "takenuser", "password123");

        Assert.False(success);
        Assert.Contains("Username", message);
    }

    [Fact]
    public async Task SignUpAsync_EmptyEmail_ReturnsFalse()
    {
        var (success, message) = await _sut.SignUpAsync("", "user", "pass");

        Assert.False(success);
        Assert.NotNull(message);
    }

    // --- SignInAsync ---

    [Fact]
    public async Task SignInAsync_ValidCredentials_ReturnsTokens()
    {
        var user = new User { Id = 1, Email = "user@example.com", Username = "user", PasswordHash = "hash", PasswordSalt = "salt" };
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { user });
        _mockPasswordHasher.Setup(h => h.VerifyPassword("password", "hash", "salt")).Returns(true);
        _mockJwtProvider.Setup(j => j.GenerateAccessToken(1, "user@example.com", It.IsAny<string>())).Returns("access-token");
        _mockJwtProvider.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _mockPasswordHasher.Setup(h => h.HashPassword("refresh-token")).Returns(("refreshhash", "refreshsalt"));
        _mockRefreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken rt) => rt);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var (success, userId, accessToken, refreshToken, expiresAt) =
            await _sut.SignInAsync("user@example.com", "password");

        Assert.True(success);
        Assert.Equal(1, userId);
        Assert.Equal("access-token", accessToken);
        Assert.Equal("refresh-token", refreshToken);
        Assert.NotNull(expiresAt);
    }

    [Fact]
    public async Task SignInAsync_EmailNotFound_ReturnsFalse()
    {
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<User>());

        var (success, userId, _, _, _) = await _sut.SignInAsync("nobody@example.com", "password");

        Assert.False(success);
        Assert.Null(userId);
    }

    [Fact]
    public async Task SignInAsync_WrongPassword_ReturnsFalse()
    {
        var user = new User { Id = 1, Email = "user@example.com", Username = "user", PasswordHash = "hash", PasswordSalt = "salt" };
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { user });
        _mockPasswordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), "hash", "salt")).Returns(false);

        var (success, userId, _, _, _) = await _sut.SignInAsync("user@example.com", "wrongpass");

        Assert.False(success);
        Assert.Null(userId);
    }

    // --- RefreshTokenAsync ---

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        var user = new User { Id = 1, Email = "user@example.com", Username = "user" };
        var tokenEntity = new RefreshToken
        {
            Id = 1, UserId = 1, TokenHash = "oldhash",
            ExpiresAtUtc = _now.AddDays(6), IsRevoked = false
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockRefreshTokenRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { tokenEntity });
        _mockPasswordHasher.Setup(h => h.VerifyPassword("old-token", "oldhash", "oldhash")).Returns(true);
        _mockRefreshTokenRepo.Setup(r => r.UpdateAsync(tokenEntity)).ReturnsAsync(tokenEntity);
        _mockJwtProvider.Setup(j => j.GenerateAccessToken(1, "user@example.com", It.IsAny<string>())).Returns("new-access");
        _mockJwtProvider.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh");
        _mockPasswordHasher.Setup(h => h.HashPassword("new-refresh")).Returns(("newhash", "newsalt"));
        _mockRefreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken rt) => rt);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var (success, accessToken, newRefreshToken, expiresAt) =
            await _sut.RefreshTokenAsync(1, "old-token");

        Assert.True(success);
        Assert.Equal("new-access", accessToken);
        Assert.Equal("new-refresh", newRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNotFound_ReturnsFalse()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var (success, _, _, _) = await _sut.RefreshTokenAsync(99, "some-token");

        Assert.False(success);
    }

    [Fact]
    public async Task RefreshTokenAsync_AllTokensExpiredOrRevoked_ReturnsFalse()
    {
        var user = new User { Id = 1, Email = "user@example.com", Username = "user" };
        var expiredToken = new RefreshToken
        {
            Id = 1, UserId = 1, TokenHash = "hash",
            ExpiresAtUtc = _now.AddDays(-1),
            IsRevoked = false
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockRefreshTokenRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { expiredToken });

        var (success, _, _, _) = await _sut.RefreshTokenAsync(1, "some-token");

        Assert.False(success);
    }

    // --- LogoutAsync ---

    [Fact]
    public async Task LogoutAsync_ValidToken_ReturnsTrue()
    {
        var tokenEntity = new RefreshToken
        {
            Id = 1, UserId = 1, TokenHash = "hash",
            ExpiresAtUtc = _now.AddDays(6), IsRevoked = false
        };
        _mockRefreshTokenRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { tokenEntity });
        _mockPasswordHasher.Setup(h => h.VerifyPassword("refresh-token", "hash", "hash")).Returns(true);
        _mockRefreshTokenRepo.Setup(r => r.UpdateAsync(tokenEntity)).ReturnsAsync(tokenEntity);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.LogoutAsync(1, "refresh-token");

        Assert.True(result);
    }

    [Fact]
    public async Task LogoutAsync_NoActiveToken_ReturnsFalse()
    {
        _mockRefreshTokenRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<RefreshToken>());

        var result = await _sut.LogoutAsync(1, "refresh-token");

        Assert.False(result);
    }

    [Fact]
    public async Task LogoutAsync_TokenHashMismatch_ReturnsFalse()
    {
        var tokenEntity = new RefreshToken
        {
            Id = 1, UserId = 1, TokenHash = "hash",
            ExpiresAtUtc = _now.AddDays(6), IsRevoked = false
        };
        _mockRefreshTokenRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { tokenEntity });
        _mockPasswordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), "hash", "hash")).Returns(false);

        var result = await _sut.LogoutAsync(1, "wrong-token");

        Assert.False(result);
    }
}
