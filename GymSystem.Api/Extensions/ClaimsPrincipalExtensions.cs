using System.Security.Claims;
using GymSystem.Application.Common;

namespace GymSystem.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetMemberId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("member_id")?.Value;

        return int.TryParse(value, out var id)
            ? id
            : throw new ForbiddenException("This account is not a gym member.");
    }

    public static int GetInstructorId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("instructor_id")?.Value;

        if (!int.TryParse(value, out var id))
            throw new ForbiddenException("This account is not linked to an instructor profile.");

        return id;
    }
}