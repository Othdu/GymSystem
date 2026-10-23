using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Application.Admin;

public interface IInstructorAccountService
{
    Task<InstructorAccountResponse> CreateAsync(CreateInstructorAccountRequest request, CancellationToken ct = default);
}

public class InstructorAccountService : IInstructorAccountService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHashing _hasher;
    private readonly IUnitOfWork _uow;

    public InstructorAccountService(IUserRepository users, IPasswordHashing hasher, IUnitOfWork uow)
    {
        _users = users;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<InstructorAccountResponse> CreateAsync(
        CreateInstructorAccountRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(email, ct))
            throw new DomainException("An account with this email already exists.");

        var instructor = new Instructor(request.FullName.Trim());
        var user = User.CreateInstructor(email, _hasher.Hash(request.Password), instructor);

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct); // one call = one transaction: profile and login both save, or neither

        return new InstructorAccountResponse(user.Id, instructor.Id, instructor.FullName, user.Email);
    }
}