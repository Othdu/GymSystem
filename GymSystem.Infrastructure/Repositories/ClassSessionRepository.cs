using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Infrastructure.Persistence;
using static System.Net.Mime.MediaTypeNames;

using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class ClassSessionRepository : IClassSessionRepository
{
    private readonly GymSystemDbContext _db;

    public ClassSessionRepository(GymSystemDbContext db) => _db = db;

    public async Task<ClassSession?> GetByIdWithBookingsAsync(int id, CancellationToken ct = default) =>
        await _db.ClassSessions
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    public async Task<bool> InstructorHasOverlapAsync(
    int instructorId, DateTime start, DateTime end, CancellationToken ct = default) =>
    await _db.ClassSessions.AnyAsync(c =>
        c.InstructorId == instructorId && c.StartTime < end && c.EndTime > start, ct);

    public async Task<List<ClassSession>> GetMemberSessionsAsync(
        int memberId, DateTime from, DateTime to, int excludeSessionId, CancellationToken ct = default) =>
        await _db.ClassSessions
            .Include(c => c.Bookings)
            .Where(c => c.Id != excludeSessionId
                     && c.StartTime >= from && c.StartTime < to
                     && c.Bookings.Any(b => b.MemberId == memberId &&
                            (b.Status == BookingStatus.Booked || b.Status == BookingStatus.Waitlisted)))
            .ToListAsync(ct);

    public async Task<List<ClassSession>> GetByBranchAndDateAsync(
        int branchId, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        return await _db.ClassSessions
            .Include(c => c.Bookings)
            .Where(c => c.BranchId == branchId && c.StartTime >= start && c.StartTime < end)
            .ToListAsync(ct);
    }
    public async Task<List<ClassSession>> GetByInstructorAndDateAsync(
    int instructorId, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        return await _db.ClassSessions
            .Include(c => c.Bookings)
            .Where(c => c.InstructorId == instructorId && c.StartTime >= start && c.StartTime < end)
            .OrderBy(c => c.StartTime)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ClassSession session, CancellationToken ct = default) =>
        await _db.ClassSessions.AddAsync(session, ct);
}