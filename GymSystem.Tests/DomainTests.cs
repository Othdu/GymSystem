using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;
using Xunit;

namespace GymSystem.Tests;

public class MembershipTests
{
    private static readonly DateTime Start = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private static Membership NewMembership(int memberId = 1) =>
        Membership.Start(memberId, new MembershipPlan("Monthly", PlanType.Monthly, 500m), Start);

    [Fact]
    public void Freeze_ActiveMembership_BecomesFrozen()
    {
        var m = NewMembership();
        m.Freeze(Start.AddDays(5));
        Assert.Equal(MembershipStatus.Frozen, m.Status);
    }

    [Fact]
    public void Freeze_SecondTime_Throws()
    {
        var m = NewMembership();
        m.Freeze(Start);
        m.Unfreeze(Start.AddDays(1));
        Assert.Throws<DomainException>(() => m.Freeze(Start.AddDays(2)));
    }

    [Fact]
    public void Unfreeze_ExtendsEndDateByFrozenDays()
    {
        var m = NewMembership();
        var originalEnd = m.EndDate;
        m.Freeze(Start.AddDays(5));
        m.Unfreeze(Start.AddDays(15)); // frozen for 10 days
        Assert.Equal(originalEnd.AddDays(10), m.EndDate);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var m = NewMembership();
        m.Cancel();
        Assert.Throws<DomainException>(() => m.Cancel());
    }
}

public class ClassSessionTests
{
    private static readonly DateTime Start = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Now = Start.AddDays(1);

    private static Membership NewMembership(int memberId) =>
        Membership.Start(memberId, new MembershipPlan("Monthly", PlanType.Monthly, 500m), Start);

    private static ClassSession NewSession(int capacity) =>
        new(1, 1, "Yoga", Start.AddDays(10), Start.AddDays(10).AddHours(1), capacity);

    [Fact]
    public void Book_WhenFull_GoesToWaitlist()
    {
        var session = NewSession(capacity: 1);
        var first = session.Book(1, NewMembership(1), Now);
        var second = session.Book(2, NewMembership(2), Now);

        Assert.Equal(BookingStatus.Booked, first.Status);
        Assert.Equal(BookingStatus.Waitlisted, second.Status);
    }

    [Fact]
    public void Book_WithFrozenMembership_Throws()
    {
        var membership = NewMembership(1);
        membership.Freeze(Start);
        var session = NewSession(capacity: 5);

        Assert.Throws<DomainException>(() => session.Book(1, membership, Now));
    }

    [Fact]
    public void Cancel_MoreThanTwoHoursBefore_PromotesWaitlist()
    {
        var session = NewSession(capacity: 1);
        session.Book(1, NewMembership(1), Now);
        var waitlisted = session.Book(2, NewMembership(2), Now);

        var promoted = session.Cancel(1, session.StartTime.AddHours(-5));

        Assert.NotNull(promoted);
        Assert.Equal(BookingStatus.Booked, waitlisted.Status);
    }

    [Fact]
    public void Cancel_LessThanTwoHoursBefore_IsLateCancelled()
    {
        var session = NewSession(capacity: 5);
        var booking = session.Book(1, NewMembership(1), Now);

        session.Cancel(1, session.StartTime.AddHours(-1));

        Assert.Equal(BookingStatus.LateCancelled, booking.Status);
    }
}