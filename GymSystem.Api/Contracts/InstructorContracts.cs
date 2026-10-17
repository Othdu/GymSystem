using FluentValidation;
using GymSystem.Domain.Enums;

namespace GymSystem.Api.Contracts;

public record MarkAttendanceBody(int MemberId, BookingStatus Outcome);

public class MarkAttendanceBodyValidator : AbstractValidator<MarkAttendanceBody>
{
    public MarkAttendanceBodyValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.Outcome)
            .Must(o => o is BookingStatus.Attended or BookingStatus.NoShow)
            .WithMessage("Outcome must be Attended or NoShow.");
    }
}