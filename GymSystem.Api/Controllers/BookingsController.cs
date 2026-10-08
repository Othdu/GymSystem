using GymSystem.Application.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        [HttpPost]
        public async Task<IActionResult> BookClass([FromBody] BookClassRequest request,CancellationToken ct)
        {
            var result = await _bookingService.BookClassAsync(request);
            return Ok(result);      
        }
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest request, CancellationToken ct)
        {
            var result = await _bookingService.CancelBookingAsync(request, ct);
            return Ok(result);
        }
    }


    
}
