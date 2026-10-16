using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHashing _hasher;
    private readonly IJwtTokenGenerator _tokens;
    private readonly IUnitOfWork _uow;

    public AuthService(
        IUserRepository users,
        IPasswordHashing hasher,
        IJwtTokenGenerator tokens,
        IUnitOfWork uow)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _uow = uow;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = Normalize(request.Email);

        if (await _users.EmailExistsAsync(email, ct))
            throw new DomainException("An account with this email already exists.");

        var member = new Member(request.FullName.Trim(), email, request.Phone.Trim());
        var user = User.CreateMember(email, _hasher.Hash(request.Password), member);

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct); // saves the member and the user together

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = Normalize(request.Email);
        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var token = _tokens.GenerateToken(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, user.Email, user.Role.ToString(), user.MemberId);
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}