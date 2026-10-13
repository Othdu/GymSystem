using GymSystem.Application.Common;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Infrastructure.Persistence;
using GymSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GymSystem.Tests;

public class ConcurrencyTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Server=localhost;Database=GymSystemDb_Tests;Trusted_Connection=True;TrustServerCertificate=True";

    private readonly DbContextOptions<GymSystemDbContext> _options =
        new DbContextOptionsBuilder<GymSystemDbContext>().UseSqlServer(ConnectionString).Options;

    public async Task InitializeAsync()
    {
        await using var db = new GymSystemDbContext(_options);
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = new GymSystemDbContext(_options);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task TwoMembersBookingTheLastSpot_SecondSaveIsRejected()
    {
        var now = DateTime.UtcNow;
        int sessionId;

        // arrange: a class with ONE spot, and two members who both have a membership
        await using (var db = new GymSystemDbContext(_options))
        {
            var plan = new MembershipPlan("Monthly", PlanType.Monthly, 500m);
            db.MembershipPlans.Add(plan);
            await db.SaveChangesAsync();

            db.Memberships.Add(Membership.Start(1, plan, now));
            db.Memberships.Add(Membership.Start(2, plan, now));

            var session = new ClassSession(1, 1, "Yoga", now.AddDays(1), now.AddDays(1).AddHours(1), capacity: 1);
            db.ClassSessions.Add(session);
            await db.SaveChangesAsync();
            sessionId = session.Id;
        }

        // two separate requests = two separate DbContexts
        await using var dbA = new GymSystemDbContext(_options);
        await using var dbB = new GymSystemDbContext(_options);

        // both load the class at the same moment, so both see an empty class
        var sessionA = await dbA.ClassSessions.Include(c => c.Bookings).FirstAsync(c => c.Id == sessionId);
        var sessionB = await dbB.ClassSessions.Include(c => c.Bookings).FirstAsync(c => c.Id == sessionId);
        var membershipA = await dbA.Memberships.FirstAsync(m => m.MemberId == 1);
        var membershipB = await dbB.Memberships.FirstAsync(m => m.MemberId == 2);

        // both think they got the last spot
        var bookingA = sessionA.Book(1, membershipA, now);
        var bookingB = sessionB.Book(2, membershipB, now);
        Assert.Equal(BookingStatus.Booked, bookingA.Status);
        Assert.Equal(BookingStatus.Booked, bookingB.Status);

        // the first save wins
        await dbA.SaveChangesAsync();

        // the second must be rejected, and arrive as OUR exception, not EF's
        await Assert.ThrowsAsync<ConcurrencyException>(() => new UnitOfWork(dbB).SaveChangesAsync());

        // only ONE booking made it into the database
        await using var check = new GymSystemDbContext(_options);
        var saved = await check.ClassSessions.Include(c => c.Bookings).SingleAsync(c => c.Id == sessionId);
        Assert.Single(saved.Bookings);
        Assert.Equal(1, saved.Bookings.First().MemberId);
    }
}