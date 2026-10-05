using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Application.Bookings;

public interface IBookingService
{
    Task<BookClassResponse> BookClassAsync(BookClassRequest request, CancellationToken ct = default);
    Task<CancelBookingResponse> CancelBookingAsync(CancelBookingRequest request, CancellationToken ct = default);
}

public class BookingService : IBookingService
{
    private readonly IClassSessionRepository _sessions;
    private readonly IMembershipRepository _memberships;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _clock;

    public BookingService(
        IClassSessionRepository sessions,
        IMembershipRepository memberships,
        IUnitOfWork uow,
        IDateTimeProvider clock)
    {
        _sessions = sessions;
        _memberships = memberships;
        _uow = uow;
        _clock = clock;
    }

    public async Task<BookClassResponse> BookClassAsync(BookClassRequest request, CancellationToken ct = default)
    {
        var session = await _sessions.GetByIdWithBookingsAsync(request.ClassSessionId, ct)
            ?? throw new NotFoundException($"Class session {request.ClassSessionId} was not found.");

        var membership = await _memberships.GetCurrentForMemberAsync(request.MemberId, ct)
            ?? throw new NotFoundException("This member has no current membership.");

        // rule that spans several sessions, so it lives here and not inside ClassSession
        var memberSessions = await _sessions.GetMemberSessionsAsync(
            request.MemberId, session.StartTime.Date, session.StartTime.Date.AddDays(1),
            excludeSessionId: request.ClassSessionId, ct);

        if (memberSessions.Any(s => s.OverlapsWith(session)))
            throw new DomainException("You already have a class that overlaps with this one.");

        var booking = session.Book(request.MemberId, membership, _clock.UtcNow);
        await _uow.SaveChangesAsync(ct);

        return new BookClassResponse(booking.Id, booking.Status);
    }

    public async Task<CancelBookingResponse> CancelBookingAsync(CancelBookingRequest request, CancellationToken ct = default)
    {
        var session = await _sessions.GetByIdWithBookingsAsync(request.ClassSessionId, ct)
            ?? throw new NotFoundException($"Class session {request.ClassSessionId} was not found.");

        var promoted = session.Cancel(request.MemberId, _clock.UtcNow);

        var cancelled = session.Bookings
            .Where(b => b.MemberId == request.MemberId && b.CancelledAt != null)
            .OrderByDescending(b => b.CancelledAt)
            .First();

        await _uow.SaveChangesAsync(ct);

        return new CancelBookingResponse(cancelled.Status, promoted?.MemberId);
    }
}
