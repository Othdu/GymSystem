using GymSystem.Application.Abstractions;
using GymSystem.Application.Timetable;
using GymSystem.Domain.Entities;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class TimetableServiceTests
{
    private static readonly DateOnly Day = new(2026, 10, 25);
    private static readonly DateTime Start = new(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc);
    private const string Key = "timetable:1:2026-10-25";

    private readonly Mock<IClassSessionRepository> _sessions = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly TimetableService _service;

    public TimetableServiceTests()
    {
        _service = new TimetableService(_sessions.Object, _cache.Object);
    }

    [Fact]
    public async Task Get_WhenCached_ReturnsCachedAndSkipsDatabase()
    {
        var cached = new List<TimetableItemResponse>
        {
            new(1, "Yoga", 1, Start, Start.AddHours(1), 10, 5)
        };
        _cache.Setup(c => c.GetAsync<List<TimetableItemResponse>>(Key, It.IsAny<CancellationToken>()))
              .ReturnsAsync(cached);

        var result = await _service.GetAsync(1, Day);

        Assert.Same(cached, result);
        _sessions.Verify(r => r.GetByBranchAndDateAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_WhenNotCached_LoadsFromDatabaseAndStoresResult()
    {
        _cache.Setup(c => c.GetAsync<List<TimetableItemResponse>>(Key, It.IsAny<CancellationToken>()))
              .ReturnsAsync((List<TimetableItemResponse>?)null);
        _sessions.Setup(r => r.GetByBranchAndDateAsync(1, Day, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<ClassSession> { new(1, 1, "Yoga", Start, Start.AddHours(1), 10) });

        var result = await _service.GetAsync(1, Day);

        Assert.Single(result);
        _cache.Verify(c => c.SetAsync(
            Key, It.IsAny<List<TimetableItemResponse>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}