using GymSystem.Application.Abstractions;

namespace GymSystem.Application.Memberships;

public interface IMembershipExpiryService
{
    Task<int> ExpireDueMembershipsAsync(CancellationToken ct = default);
}

public class MembershipExpiryService : IMembershipExpiryService
{
    private readonly IMembershipRepository _memberships;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _clock;

    public MembershipExpiryService(
        IMembershipRepository memberships, IUnitOfWork uow, IDateTimeProvider clock)
    {
        _memberships = memberships;
        _uow = uow;
        _clock = clock;
    }

    public async Task<int> ExpireDueMembershipsAsync(CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var due = await _memberships.GetActiveEndedOnOrBeforeAsync(now, ct);

        foreach (var membership in due)
            membership.Expire(now);

        if (due.Count > 0)
            await _uow.SaveChangesAsync(ct);

        return due.Count;
    }
}