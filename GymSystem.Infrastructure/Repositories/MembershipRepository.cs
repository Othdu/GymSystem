using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly GymSystemDbContext _db;

    public MembershipRepository(GymSystemDbContext db) => _db = db;

    public async Task<Membership?> GetCurrentForMemberAsync(int memberId, CancellationToken ct = default) =>
        await _db.Memberships
            .Where(m => m.MemberId == memberId &&
                       (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Membership membership, CancellationToken ct = default) =>
        await _db.Memberships.AddAsync(membership, ct);
}
