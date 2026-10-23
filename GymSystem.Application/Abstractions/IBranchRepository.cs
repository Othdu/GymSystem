using GymSystem.Domain.Entities;

namespace GymSystem.Application.Abstractions;

public interface IBranchRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Branch branch, CancellationToken ct = default);
}