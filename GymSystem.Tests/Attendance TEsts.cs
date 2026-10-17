using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using GymSystem.Application.Instructors;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class ClassSessionAttendanceTests
{
    private static readonly DateTime Start = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime BookedAt = Start.AddDays(1);

    private static Membership NewMembership(int memberId) =>
        Membership.Start(memberId, new MembershipPlan("Monthly", PlanType.Monthly, 500m), Start);

    private static ClassSession NewSession(int capacity = 5) =>
        new(1, 1, "Yoga", Start.AddDays(10), Start.AddDays(10).AddHours(1), capacity);

    [Fact]
    public void MarkAttendance_AfterClassStarts_SetsAttended()
    {
        var session = NewSession();
        var booking = session.Book(1, NewMembership(1), BookedAt);

        session.MarkAttendance(1, BookingStatus.Attended, session.StartTime.AddMinutes(5));

        Assert.Equal(BookingStatus.Attended, booking.Status);
    }

    [Fact]
    public void MarkAttendance_BeforeClassStarts_Throws()
    {
        var session = NewSession();
        session.Book(1, NewMembership(1), BookedAt);

        Assert.Throws<DomainException>(() =>
            session.MarkAttendance(1, BookingStatus.Attended, session.StartTime.AddMinutes(-5)));
    }

    [Fact]
    public void MarkAttendance_WithInvalidOutcome_Throws()
    {
        var session = NewSession();
        session.Book(1, NewMembership(1), BookedAt);

        Assert.Throws<DomainException>(() =>
            session.MarkAttendance(1, BookingStatus.Cancelled, session.StartTime.AddMinutes(5)));
    }

    [Fact]
    public void MarkAttendance_ForMemberWithoutBooking_Throws()
    {
        var session = NewSession();

        Assert.Throws<DomainException>(() =>
            session.MarkAttendance(99, BookingStatus.Attended, session.StartTime.AddMinutes(5)));
    }

    [Fact]
    public void MarkAttendance_ForWaitlistedMember_Throws()
    {
        var session = NewSession(capacity: 1);
        session.Book(1, NewMembership(1), BookedAt);
        session.Book(2, NewMembership(2), BookedAt); // waitlisted

        Assert.Throws<DomainException>(() =>
            session.MarkAttendance(2, BookingStatus.Attended, session.StartTime.AddMinutes(5)));
    }

    [Fact]
    public void MarkAttendance_Twice_Throws()
    {
        var session = NewSession();
        session.Book(1, NewMembership(1), BookedAt);
        session.MarkAttendance(1, BookingStatus.NoShow, session.StartTime.AddMinutes(5));

        Assert.Throws<DomainException>(() =>
            session.MarkAttendance(1, BookingStatus.Attended, session.StartTime.AddMinutes(6)));
    }
}

public class InstructorServiceTests
{
    private static readonly DateTime Start = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IClassSessionRepository> _sessions = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly InstructorService _service;

    public InstructorServiceTests()
    {
        _service = new InstructorService(_sessions.Object, _uow.Object, _clock.Object);
    }

    private ClassSession SessionOwnedBy(int instructorId)
    {
        var session = new ClassSession(1, instructorId, "Yoga", Start.AddDays(10), Start.AddDays(10).AddHours(1), 5);
        var membership = Membership.Start(1, new MembershipPlan("Monthly", PlanType.Monthly, 500m), Start);
        session.Book(1, membership, Start.AddDays(1));

        _sessions.Setup(r => r.GetByIdWithBookingsAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(session);
        _clock.Setup(c => c.UtcNow).Returns(session.StartTime.AddMinutes(5));
        return session;
    }

    [Fact]
    public async Task MarkAttendance_OnAnotherInstructorsClass_ThrowsForbidden()
    {
        SessionOwnedBy(instructorId: 2);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.MarkAttendanceAsync(1, new MarkAttendanceRequest(10, 1, BookingStatus.Attended)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAttendance_OnOwnClass_SavesAndReturnsNewStatus()
    {
        SessionOwnedBy(instructorId: 1);

        var response = await _service.MarkAttendanceAsync(1, new MarkAttendanceRequest(10, 1, BookingStatus.Attended));

        Assert.Equal(BookingStatus.Attended, response.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendance_ForUnknownClass_ThrowsNotFound()
    {
        _sessions.Setup(r => r.GetByIdWithBookingsAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ClassSession?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.MarkAttendanceAsync(1, new MarkAttendanceRequest(10, 1, BookingStatus.Attended)));
    }
}