using System.Globalization;

namespace GymSystem.Application.Timetable;

public static class TimetableCacheKeys
{
    public static string For(int branchId, DateOnly date) =>
        $"timetable:{branchId}:{date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
}