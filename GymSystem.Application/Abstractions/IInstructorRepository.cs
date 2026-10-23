namespace GymSystem.Application.Abstractions;

public interface IInstructorRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}