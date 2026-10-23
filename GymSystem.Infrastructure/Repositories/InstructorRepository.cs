using GymSystem.Application.Abstractions;
using GymSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly GymSystemDbContext _db;

    public InstructorRepository(GymSystemDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        await _db.Instructors.AnyAsync(i => i.Id == id, ct);
}