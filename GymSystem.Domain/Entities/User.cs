using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public int? MemberId { get; private set; }
    public Member? Member { get; private set; }
    public int? InstructorId { get; private set; }
    public Instructor? Instructor { get; private set; }

    private User() { }

    public static User CreateMember(string email, string passwordHash, Member member)
    {
        Validate(email, passwordHash);

        return new User
        {
            Email = Normalize(email),
            PasswordHash = passwordHash,
            Role = UserRole.Member,
            Member = member
        };
    }
    public static User CreateInstructor(string email, string passwordHash, Instructor instructor)
    {
        Validate(email, passwordHash);

        return new User
        {
            Email = Normalize(email),
            PasswordHash = passwordHash,
            Role = UserRole.Instructor,
            Instructor = instructor
        };
    }

    public static User CreateAdmin(string email, string passwordHash)
    {
        Validate(email, passwordHash);

        return new User
        {
            Email = Normalize(email),
            PasswordHash = passwordHash,
            Role = UserRole.Admin
        };
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private static void Validate(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");
    }
}