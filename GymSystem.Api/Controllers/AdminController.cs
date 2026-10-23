using GymSystem.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminCatalogService _catalog;
    private readonly IInstructorAccountService _instructorAccounts;

    public AdminController(IAdminCatalogService catalog, IInstructorAccountService instructorAccounts)
    {
        _catalog = catalog;
        _instructorAccounts = instructorAccounts;
    }

    [HttpPost("branches")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _catalog.CreateBranchAsync(request, ct));

    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _catalog.CreatePlanAsync(request, ct));

    [HttpPost("classes")]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _catalog.CreateClassAsync(request, ct));

    [HttpPost("instructors")]
    public async Task<IActionResult> CreateInstructor(
        [FromBody] CreateInstructorAccountRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _instructorAccounts.CreateAsync(request, ct));
}