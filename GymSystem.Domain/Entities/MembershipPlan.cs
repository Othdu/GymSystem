using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Domain.Entities
{
    public class MembershipPlan
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public PlanType Type { get; private set; }
        public decimal Price { get; private set; }
        private MembershipPlan() { }

        public MembershipPlan(string name, PlanType type,decimal price)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("plan name is requerd");
            if (price <= 0) throw new DomainException("Plan price must be greater than zero");
            Name = name;
            Type = type;
            Price = price;
        }
        public int DurationDays => Type switch
        {
            PlanType.Monthly =>30,
            PlanType.Quarterly =>90,
            PlanType.Annual =>365,
            _ => throw new DomainException("Unknown plan type.")
        };
    }
}
