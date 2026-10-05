using GymSystem.Application.Abstractions;
using GymSystem.Application.Bookings;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class BookingServiceTests
{
    private static readonly DateTime Start = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Now = Start.AddDays(1);

    private readonly Mock<IClassSessionRepository> _sessions = new();
    private readonly Mock<IMembershipRepository> _memberships = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(Now);
        _service = new BookingService(_sessions.Object, _memberships.Object, _uow.Object, _clock.Object);
    }

    private static Membership NewMembership(int memberId) =>
        Membership.Start(memberId, new MembershipPlan("Monthly", PlanType.Monthly, 500m), Start);

    [Fact]
    public async Task BookClass_WhenMemberHasOverlappingClass_Throws()
    {
        var target = new ClassSession(1, 1, "Yoga", Start.AddDays(10), Start.AddDays(10).AddHours(1), 10);
        var other = new ClassSession(1, 2, "Spinning", Start.AddDays(10).AddMinutes(30), Start.AddDays(10).AddHours(2), 10);

        _sessions.Setup(r => r.GetByIdWithBookingsAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);
        _sessions.Setup(r => r.GetMemberSessionsAsync(
                        1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<ClassSession> { other });
        _memberships.Setup(r => r.GetCurrentForMemberAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(NewMembership(1));

        await Assert.ThrowsAsync<DomainException>(
            () => _service.BookClassAsync(new BookClassRequest(1, 10)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelBooking_PromotesFirstWaitlistedMember()
    {
        var session = new ClassSession(1, 1, "Yoga", Start.AddDays(10), Start.AddDays(10).AddHours(1), 1);
        session.Book(1, NewMembership(1), Now);
        session.Book(2, NewMembership(2), Now); // waitlisted, class has 1 spot

        _sessions.Setup(r => r.GetByIdWithBookingsAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(session);

        var response = await _service.CancelBookingAsync(new CancelBookingRequest(1, 10));

        Assert.Equal(BookingStatus.Cancelled, response.CancelledAs);
        Assert.Equal(2, response.PromotedMemberId);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}