using GymSystem.Api.Contracts;
using GymSystem.Api.Extensions;
using GymSystem.Application.Instructors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[ApiController]
[Route("api/instructor")]
[Authorize(Roles = "Instructor")]
public class InstructorController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetMyClasses([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _instructorService.GetMyClassesAsync(User.GetInstructorId(), day, ct);
        return Ok(result);
    }

    [HttpPut("classes/{classSessionId:int}/attendance")]
    public async Task<IActionResult> MarkAttendance(
        int classSessionId, [FromBody] MarkAttendanceBody body, CancellationToken ct)
    {
        var request = new MarkAttendanceRequest(classSessionId, body.MemberId, body.Outcome);
        var result = await _instructorService.MarkAttendanceAsync(User.GetInstructorId(), request, ct);
        return Ok(result);
    }
}