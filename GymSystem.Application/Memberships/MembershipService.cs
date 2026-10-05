using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using GymSystem.Application.Memberships;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Exceptions;
namespace GymSystem.Application.Memberships;

public interface IMembershipService
{
    Task<MembershipResponse> PurchaseAsync(PurchaseMembershipRequest request, CancellationToken ct = default);
    Task<MembershipResponse> FreezeAsync(int memberId, CancellationToken ct = default);
    Task<MembershipResponse> UnfreezeAsync(int memberId, CancellationToken ct = default);
}

public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _memberships;
    private readonly IMembershipPlanRepository _plans;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _clock;

    public MembershipService(
        IMembershipRepository memberships,
        IMembershipPlanRepository plans,
        IUnitOfWork uow,
        IDateTimeProvider clock)
    {
        _memberships = memberships;
        _plans = plans;
        _uow = uow;
        _clock = clock;
    }

    public async Task<MembershipResponse> PurchaseAsync(PurchaseMembershipRequest request, CancellationToken ct = default)
    {
        var plan = await _plans.GetByIdAsync(request.PlanId, ct)
            ?? throw new NotFoundException($"Plan {request.PlanId} was not found.");

        var current = await _memberships.GetCurrentForMemberAsync(request.MemberId, ct);
        if (current is not null)
            throw new DomainException("This member already has a current membership.");

        var membership = GymSystem.Domain.Entities.Membership.Start(request.MemberId, plan, _clock.UtcNow);
        await _memberships.AddAsync(membership, ct);
        await _uow.SaveChangesAsync(ct);

        return ToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int memberId, CancellationToken ct = default)
    {
        var membership = await GetCurrentOrThrowAsync(memberId, ct);
        membership.Freeze(_clock.UtcNow);
        await _uow.SaveChangesAsync(ct);
        return ToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int memberId, CancellationToken ct = default)
    {
        var membership = await GetCurrentOrThrowAsync(memberId, ct);
        membership.Unfreeze(_clock.UtcNow);
        await _uow.SaveChangesAsync(ct);
        return ToResponse(membership);
    }

    private async Task<Membership> GetCurrentOrThrowAsync(int memberId, CancellationToken ct) =>
        await _memberships.GetCurrentForMemberAsync(memberId, ct)
            ?? throw new NotFoundException("This member has no current membership.");

    private static MembershipResponse ToResponse(Membership m) =>
        new(m.Id, m.MemberId, m.PlanId, m.StartDate, m.EndDate, m.Status);
}