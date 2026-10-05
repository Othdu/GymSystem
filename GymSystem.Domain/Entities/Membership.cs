using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;


namespace GymSystem.Domain.Entities
{
    public class Membership
    {
        public const int MaxFreezingDays = 30;
        public int Id { get; private set; }
        public int MemberId { get; private set; }

        public int PlanId {get; private set;}
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public MembershipStatus Status { get; private set; }    
        public DateTime?  FrozenAt { get; private set; }
        public bool HasUsedFreeze {  get; private set; }

        private Membership() { }

        public static Membership Start(int memberId, MembershipPlan plan, DateTime startDate) => new()
        {
            MemberId = memberId,
            PlanId = plan.Id,
            StartDate = startDate,
            EndDate = startDate.AddDays(plan.DurationDays),
            Status = MembershipStatus.Active

        };
         
        public bool IsValidOn(DateTime date) =>
            Status == MembershipStatus.Active&& date>=StartDate&&date<=EndDate;
        public void Freeze (DateTime now)
        {
            if (Status != MembershipStatus.Active) throw new DomainException("Only an acitve plan can be frozen.");
            if (HasUsedFreeze)
                throw new DomainException("you can only use freeze once");

            Status = MembershipStatus.Frozen;
            FrozenAt = now;
            HasUsedFreeze =true;


        }
        public void Unfreeze(DateTime now)
        {
            if (Status != MembershipStatus.Frozen)
                throw new DomainException("Only a frozen membership can be unfrozen");
            var frozenDays = (int)Math.Min((now - FrozenAt!.Value).TotalDays, MaxFreezingDays);
            EndDate=EndDate.AddDays(frozenDays);
            Status = MembershipStatus.Active;
            FrozenAt = null;
        }
        public void Expire(DateTime now)
        {
            if (Status!= MembershipStatus.Active)
                throw new DomainException("ONly an active membership can  expire.");
            if (now <EndDate)
                throw new DomainException("Membership has not reached its end date yet");

            Status= MembershipStatus.Expired;
        }
        public void Cancel()
        {
            if (Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
                throw new DomainException($"cannot cancel a membershup that is already{Status}.");
            Status = MembershipStatus.Cancelled;


        }


    }
}
