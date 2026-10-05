using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Domain.Entities
{
    public class Booking
    {
        public int Id {get; private set;}
        public int ClassSessionId { get; private set;}
        public int MemberId {  get; private set;}
        public BookingStatus Status { get; private set;}
        public DateTime CreatedAt { get; private set;}
        public DateTime? CancelledAt { get; private set;}

        private Booking() { }

        internal static Booking Create(int classSessionId, int memberId, BookingStatus status, DateTime now) => new()
        {
            ClassSessionId = classSessionId,
            MemberId = memberId,
            Status = status,
            CreatedAt = now

        };
        internal void Promote()
        {
            if (Status !=BookingStatus.Waitlisted)
            throw new DomainException("Only waitlisted bookings can be promoted to booked.");
            Status = BookingStatus.Booked;
        }
        internal void Cancel(bool isLate ,DateTime now)
        {
            
            Status = isLate?BookingStatus.LateCancelled:BookingStatus.Cancelled;
            CancelledAt = now;
        }



    }
}
