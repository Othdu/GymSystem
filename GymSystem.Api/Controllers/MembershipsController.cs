using GymSystem.Api.Contracts;
using GymSystem.Api.Extensions;
using GymSystem.Application.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Member")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipsController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpPost]
    public async Task<IActionResult> Purchase([FromBody] PurchaseMembershipBody body, CancellationToken ct)
    {
        var request = new PurchaseMembershipRequest(User.GetMemberId(), body.PlanId);
        var result = await _membershipService.PurchaseAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze(CancellationToken ct)
    {
        var result = await _membershipService.FreezeAsync(User.GetMemberId(), ct);
        return Ok(result);
    }

    [HttpPost("unfreeze")]
    public async Task<IActionResult> Unfreeze(CancellationToken ct)
    {
        var result = await _membershipService.UnfreezeAsync(User.GetMemberId(), ct);
        return Ok(result);
    }
}