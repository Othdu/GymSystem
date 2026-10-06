using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Infrastructure.Persistence.Configurations
{
    public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m =>m.Status).HasConversion<string>().HasMaxLength(20);
            builder.HasOne < MembershipPlan>().WithMany().HasForeignKey(m => m.PlanId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => m.MemberId);


        }

    }
}
