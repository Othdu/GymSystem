using GymSystem.Domain.Entities;

namespace GymSystem.Application.Abstractions;

public interface IMembershipRepository
{
    Task<List<Membership>> GetActiveEndedOnOrBeforeAsync(DateTime now, CancellationToken ct = default);
    Task<GymSystem.Domain.Entities.Membership?> GetCurrentForMemberAsync(int memberId, CancellationToken ct = default);
    Task AddAsync(GymSystem.Domain.Entities.Membership membership, CancellationToken ct = default);
}