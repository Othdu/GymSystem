using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly GymSystemDbContext _db;

    public BranchRepository(GymSystemDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        await _db.Branches.AnyAsync(b => b.Id == id, ct);

    public async Task AddAsync(Branch branch, CancellationToken ct = default) =>
        await _db.Branches.AddAsync(branch, ct);
}