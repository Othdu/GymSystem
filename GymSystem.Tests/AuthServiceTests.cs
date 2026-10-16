using GymSystem.Application.Abstractions;
using GymSystem.Application.Auth;
using GymSystem.Application.Common;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHashing> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokens = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
        _tokens.Setup(t => t.GenerateToken(It.IsAny<User>()))
               .Returns(new TokenResult("fake-token", DateTime.UtcNow.AddHours(1)));

        _service = new AuthService(_users.Object, _hasher.Object, _tokens.Object, _uow.Object);
    }

    private static RegisterRequest NewRegisterRequest() =>
        new("Ali Mostafa", "Ali@Example.com", "01000000001", "Passw0rd123");

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_Throws()
    {
        _users.Setup(u => u.EmailExistsAsync("ali@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        await Assert.ThrowsAsync<DomainException>(() => _service.RegisterAsync(NewRegisterRequest()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WithNewEmail_StoresHashNotPassword_AndSavesOnce()
    {
        User? saved = null;
        _users.Setup(u => u.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        _users.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .Callback<User, CancellationToken>((user, _) => saved = user)
              .Returns(Task.CompletedTask);

        var response = await _service.RegisterAsync(NewRegisterRequest());

        Assert.NotNull(saved);
        Assert.Equal("hashed-password", saved!.PasswordHash);
        Assert.Equal("ali@example.com", saved.Email);
        Assert.Equal(UserRole.Member, saved.Role);
        Assert.Equal("fake-token", response.Token);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        var user = User.CreateAdmin("admin@example.com", "stored-hash");
        _users.Setup(u => u.GetByEmailAsync("admin@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "stored-hash")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.LoginAsync(new LoginRequest("admin@example.com", "wrong")));
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ThrowsUnauthorized()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.LoginAsync(new LoginRequest("nobody@example.com", "whatever")));
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        var user = User.CreateAdmin("admin@example.com", "stored-hash");
        _users.Setup(u => u.GetByEmailAsync("admin@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("correct", "stored-hash")).Returns(true);

        var response = await _service.LoginAsync(new LoginRequest("Admin@Example.com", "correct"));

        Assert.Equal("fake-token", response.Token);
        Assert.Equal("Admin", response.Role);
    }
}