using System.Security.Claims;
using System.Text;
using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GymSystem.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly IDateTimeProvider _clock;

    public JwtTokenGenerator(JwtSettings settings, IDateTimeProvider clock)
    {
        _settings = settings;
        _clock = clock;
    }

    public TokenResult GenerateToken(User user)
    {
        var expires = _clock.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new("role", user.Role.ToString())
        };

        if (user.MemberId is not null)
            claims.Add(new Claim("member_id", user.MemberId.Value.ToString()));
        if (user.InstructorId is not null)
            claims.Add(new Claim("instructor_id", user.InstructorId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResult(token, expires);
    }
}