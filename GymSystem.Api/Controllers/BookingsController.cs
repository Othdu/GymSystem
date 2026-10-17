using GymSystem.Api.Contracts;
using GymSystem.Api.Extensions;
using GymSystem.Application.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Member")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> BookClass([FromBody] BookClassBody body, CancellationToken ct)
    {
        var request = new BookClassRequest(User.GetMemberId(), body.ClassSessionId);
        var result = await _bookingService.BookClassAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelBooking([FromBody] CancelBookingBody body, CancellationToken ct)
    {
        var request = new CancelBookingRequest(User.GetMemberId(), body.ClassSessionId);
        var result = await _bookingService.CancelBookingAsync(request, ct);
        return Ok(result);
    }
}