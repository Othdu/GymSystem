using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Domain.Entities
{
    public class ClassSession
    {
        public static readonly TimeSpan LateCancellationThreshold = TimeSpan.FromHours(2);

        private readonly List<Booking> _bookings = new();
        public int Id { get; private set; }
        public int BranchId { get; private set; }
        public int InstructorId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public DateTime? LastModified { get; private set; }
        public int Capacity { get; private set; }

        public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();
        private ClassSession() { }

        public ClassSession(int branchId, int instructorId, string title, DateTime startTime, DateTime endTime, int capacity)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Title cannot be empty.");
            if (endTime <= startTime)
                throw new DomainException("End time must be after start time.");
            if (capacity <= 0)
                throw new DomainException("Capacity must be greater than zero.");
           
            BranchId = branchId;
            InstructorId = instructorId;
            Title = title;
            StartTime = startTime;
            EndTime = endTime;
            Capacity = capacity;
        }
        public int BookedCount => _bookings.Count(b => b.Status == BookingStatus.Booked);
        public bool IsFull => BookedCount >= Capacity;
        public bool OverlapsWith(ClassSession other) => StartTime < other.EndTime && EndTime > other.StartTime;

        public Booking Book(int memberId, Membership membership, DateTime now)
        {
            if (now >= StartTime)
                throw new DomainException("This class has already started.");
            if (membership.MemberId != memberId)
                throw new DomainException("Membership does not belong to this member.");
            if (!membership.IsValidOn(StartTime))
                throw new DomainException("An active membership covering the class date is required to book.");
            if (HasActiveBooking(memberId))
                throw new DomainException("Member already has a booking for this class.");

            var status = IsFull ? BookingStatus.Waitlisted : BookingStatus.Booked;
            var booking = Booking.Create(Id, memberId, status, now);
            _bookings.Add(booking);
            LastModified = now;
            return booking;
        }
        public Booking? Cancel(int memberId, DateTime now)
        {
            if (now >= StartTime)
                throw new DomainException("This class has already started.");
            var booking = _bookings.FirstOrDefault(b => b.MemberId == memberId && b.Status != BookingStatus.Cancelled) ?? throw new DomainException("No active booking found for this member.");
            var WasBooked = booking.Status == BookingStatus.Booked;
            var isLate = StartTime - now < LateCancellationThreshold;
            booking.Cancel(isLate, now);
            LastModified = now; 

            if (!WasBooked) return null;

            var next = _bookings.Where(b => b.Status == BookingStatus.Waitlisted).OrderBy(b => b.CreatedAt).OrderBy(b => b.CreatedAt).FirstOrDefault();
            next?.Promote();
            return next;
        }

        private bool HasActiveBooking(int memberId) => _bookings.Any(b => b.MemberId == memberId && (b.Status is BookingStatus.Booked or BookingStatus.Waitlisted));

    }
}
