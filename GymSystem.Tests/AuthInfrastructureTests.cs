using System.Text;
using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Infrastructure.Auth;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class AuthInfrastructureTests
{
    private static JwtSettings NewSettings() => new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        Secret = new string('k', 40),
        ExpiryMinutes = 60
    };

    private static JwtTokenGenerator NewGenerator()
    {
        var clock = new Mock<IDateTimeProvider>();
        clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
        return new JwtTokenGenerator(NewSettings(), clock.Object);
    }

    [Fact]
    public void Hasher_AcceptsCorrectPassword_AndRejectsWrongOne()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("Passw0rd123");

        Assert.NotEqual("Passw0rd123", hash);
        Assert.True(hasher.Verify("Passw0rd123", hash));
        Assert.False(hasher.Verify("WrongPassword1", hash));
    }

    [Fact]
    public void Hasher_SamePasswordTwice_GivesDifferentHashes()
    {
        var hasher = new BCryptPasswordHasher();

        Assert.NotEqual(hasher.Hash("Passw0rd123"), hasher.Hash("Passw0rd123"));
    }

    [Fact]
    public void Token_ContainsTheUsersClaims()
    {
        var user = User.CreateAdmin("admin@example.com", "hash");

        var result = NewGenerator().GenerateToken(user);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(result.Token);

        Assert.Equal("admin@example.com", jwt.GetClaim("email").Value);
        Assert.Equal("Admin", jwt.GetClaim("role").Value);
        Assert.Equal("test-issuer", jwt.Issuer);
    }

    [Fact]
    public async Task Token_IsAcceptedWithTheRightKey_AndRejectedWithAWrongKey()
    {
        var user = User.CreateAdmin("admin@example.com", "hash");
        var token = NewGenerator().GenerateToken(user).Token;
        var handler = new JsonWebTokenHandler();

        TokenValidationParameters ParametersFor(string secret) => new()
        {
            ValidIssuer = "test-issuer",
            ValidAudience = "test-audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };

        var good = await handler.ValidateTokenAsync(token, ParametersFor(new string('k', 40)));
        var bad = await handler.ValidateTokenAsync(token, ParametersFor(new string('x', 40)));

        Assert.True(good.IsValid);
        Assert.False(bad.IsValid);
    }
}