using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using GymSystem.Domain.Enums;

namespace GymSystem.Application.Memberships
{
    
    public record PurchaseMembershipRequest(int MemberId,int PlanId);
    public record MembershipResponse(int Id, int MemberId, int PlanId, DateTime StartDate, DateTime EndDate, MembershipStatus Status);

    public class PurchaseMembershipRequestValidator : AbstractValidator<PurchaseMembershipRequest>

    {
        public PurchaseMembershipRequestValidator()
        {
            RuleFor(x => x.MemberId).GreaterThan(0);
            RuleFor(x => x.PlanId).GreaterThan(0);
        }
    }
}

  