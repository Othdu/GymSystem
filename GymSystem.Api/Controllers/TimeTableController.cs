using GymSystem.Application.Timetable;
using Microsoft.AspNetCore.Mvc;
namespace GymSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeTableController : ControllerBase
    {
        private readonly ITimetableService _timetableService;
        public TimeTableController(ITimetableService timeTableService)
        {
            _timetableService = timeTableService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int branchId, [FromQuery] DateOnly date, CancellationToken ct)
        {
            var result = await _timetableService.GetAsync(branchId, date, ct);
            return Ok(result);
        }
    }
}
