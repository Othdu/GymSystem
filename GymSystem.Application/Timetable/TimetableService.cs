using GymSystem.Application.Abstractions;
using GymSystem.Application.Timetable;

namespace GymSystem.Application.Timetable;

public interface ITimetableService
{
    Task<List<TimetableItemResponse>> GetAsync(int branchId, DateOnly date, CancellationToken ct = default);
}

public class TimetableService : ITimetableService
{
    private readonly IClassSessionRepository _sessions;

    public TimetableService(IClassSessionRepository sessions) => _sessions = sessions;

    public async Task<List<TimetableItemResponse>> GetAsync(int branchId, DateOnly date, CancellationToken ct = default)
    {
        var sessions = await _sessions.GetByBranchAndDateAsync(branchId, date, ct);

        return sessions
            .OrderBy(s => s.StartTime)
            .Select(s => new TimetableItemResponse(
                s.Id, s.Title, s.InstructorId, s.StartTime, s.EndTime,
                s.Capacity, s.Capacity - s.BookedCount))
            .ToList();
    }
}