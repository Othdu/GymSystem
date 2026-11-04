using GymSystem.Application.Abstractions;

namespace GymSystem.Application.Timetable;

public interface ITimetableService
{
    Task<List<TimetableItemResponse>> GetAsync(int branchId, DateOnly date, CancellationToken ct = default);
}

public class TimetableService : ITimetableService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IClassSessionRepository _sessions;
    private readonly ICacheService _cache;

    public TimetableService(IClassSessionRepository sessions, ICacheService cache)
    {
        _sessions = sessions;
        _cache = cache;
    }

    public async Task<List<TimetableItemResponse>> GetAsync(
        int branchId, DateOnly date, CancellationToken ct = default)
    {
        var key = TimetableCacheKeys.For(branchId, date);

        var cached = await _cache.GetAsync<List<TimetableItemResponse>>(key, ct);
        if (cached is not null)
            return cached;

        var sessions = await _sessions.GetByBranchAndDateAsync(branchId, date, ct);

        var result = sessions
            .OrderBy(s => s.StartTime)
            .Select(s => new TimetableItemResponse(
                s.Id, s.Title, s.InstructorId, s.StartTime, s.EndTime,
                s.Capacity, s.Capacity - s.BookedCount))
            .ToList();

        await _cache.SetAsync(key, result, CacheTtl, ct);
        return result;
    }
}