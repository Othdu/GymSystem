using GymSystem.Application.Abstractions;
using GymSystem.Application.Memberships;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class MembershipExpiryServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 24, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IMembershipRepository> _memberships = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly MembershipExpiryService _service;

    public MembershipExpiryServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(Now);
        _service = new MembershipExpiryService(_memberships.Object, _uow.Object, _clock.Object);
    }

    // a monthly membership that started 40 days ago ended 10 days ago
    private static Membership EndedMembership(int memberId) =>
        Membership.Start(memberId, new MembershipPlan("Monthly", PlanType.Monthly, 500m), Now.AddDays(-40));

    [Fact]
    public async Task ExpireDueMemberships_ExpiresEachOneAndSavesOnce()
    {
        var a = EndedMembership(1);
        var b = EndedMembership(2);
        _memberships.Setup(r => r.GetActiveEndedOnOrBeforeAsync(Now, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Membership> { a, b });

        var count = await _service.ExpireDueMembershipsAsync();

        Assert.Equal(2, count);
        Assert.Equal(MembershipStatus.Expired, a.Status);
        Assert.Equal(MembershipStatus.Expired, b.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExpireDueMemberships_WhenNothingIsDue_DoesNotSave()
    {
        _memberships.Setup(r => r.GetActiveEndedOnOrBeforeAsync(Now, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Membership>());

        var count = await _service.ExpireDueMembershipsAsync();

        Assert.Equal(0, count);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}