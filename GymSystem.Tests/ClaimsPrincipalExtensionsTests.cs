using System.Security.Claims;
using GymSystem.Api.Extensions;
using GymSystem.Application.Common;
using Xunit;

namespace GymSystem.Tests;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal PrincipalWith(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));

    [Fact]
    public void GetMemberId_ReadsTheMemberIdClaim()
    {
        var user = PrincipalWith(new Claim("member_id", "5"));

        Assert.Equal(5, user.GetMemberId());
    }

    [Fact]
    public void GetMemberId_WithoutTheClaim_ThrowsForbidden()
    {
        var user = PrincipalWith(new Claim("role", "Admin"));

        Assert.Throws<ForbiddenException>(() => user.GetMemberId());
    }
}