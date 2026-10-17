using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using GymSystem.Domain.Enums;

namespace GymSystem.Application.Instructors;

public interface IInstructorService
{
    Task<List<InstructorClassResponse>> GetMyClassesAsync(int instructorId, DateOnly date, CancellationToken ct = default);
    Task<MarkAttendanceResponse> MarkAttendanceAsync(int instructorId, MarkAttendanceRequest request, CancellationToken ct = default);
}

public class InstructorService : IInstructorService
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _clock;

    public InstructorService(IClassSessionRepository sessions, IUnitOfWork uow, IDateTimeProvider clock)
    {
        _sessions = sessions;
        _uow = uow;
        _clock = clock;
    }

    public async Task<List<InstructorClassResponse>> GetMyClassesAsync(
        int instructorId, DateOnly date, CancellationToken ct = default)
    {
        var sessions = await _sessions.GetByInstructorAndDateAsync(instructorId, date, ct);

        return sessions.Select(s => new InstructorClassResponse(
            s.Id, s.Title, s.StartTime, s.EndTime, s.Capacity,
            s.Bookings
                .Where(b => b.Status is BookingStatus.Booked or BookingStatus.Attended or BookingStatus.NoShow)
                .Select(b => new AttendeeResponce(b.MemberId, b.Status))
                .ToList()))
            .ToList();
    }

    public async Task<MarkAttendanceResponse> MarkAttendanceAsync(
        int instructorId, MarkAttendanceRequest request, CancellationToken ct = default)
    {
        var session = await _sessions.GetByIdWithBookingsAsync(request.ClassSessionId, ct)
            ?? throw new NotFoundException($"Class session {request.ClassSessionId} was not found.");

        // resource-based authorization: the role says "instructor", this says "YOUR class"
        if (session.InstructorId != instructorId)
            throw new ForbiddenException("You can only manage attendance for your own classes.");

        var booking = session.MarkAttendance(request.MemberId, request.Outcome, _clock.UtcNow);
        await _uow.SaveChangesAsync(ct);

        return new MarkAttendanceResponse(session.Id, booking.MemberId, booking.Status);
    }
}