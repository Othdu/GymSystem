using FluentValidation;
using GymSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using GymSystem.Domain.Enums;

namespace GymSystem.Application.Bookings
{
    public record BookClassRequest(int MemberId, int ClassSessionId);
    public record BookClassResponse(int BookingId, BookingStatus Status);
    public record CancelBookingRequest(int MemberId, int ClassSessionId);
    public record CancelBookingResponse(BookingStatus CancelledAs, int? PromotedMemberId);
   public class BookClassRequestValidator : AbstractValidator<BookClassRequest>
{
    public BookClassRequestValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.ClassSessionId).GreaterThan(0);
    }
}  

public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.ClassSessionId).GreaterThan(0);
    }
}
}