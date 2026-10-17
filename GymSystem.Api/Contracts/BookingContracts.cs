
using FluentValidation;


namespace GymSystem.Api.Contracts
{
    public record BookClassBody(int ClassSessionId);
    public record CancelBookingBody(int ClassSessionId);
    public record PurchaseMembershipBody(int PlanId);

    public class BookClassBodyValidator : AbstractValidator<BookClassBody>

    {
        public BookClassBodyValidator() => RuleFor(x => x.ClassSessionId).GreaterThan(0);
    }
    public class CancelBookingBodyValidator : AbstractValidator<CancelBookingBody>
    {
        public CancelBookingBodyValidator() => RuleFor(x => x.ClassSessionId).GreaterThan(0);

    }
    public class PurchaseMembershipBodyValidator : AbstractValidator<PurchaseMembershipBody>
    {
        public PurchaseMembershipBodyValidator() => RuleFor(x => x.PlanId).GreaterThan(0);
    }

}
