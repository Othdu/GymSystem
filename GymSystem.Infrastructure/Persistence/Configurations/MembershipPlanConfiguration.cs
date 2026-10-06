using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Persistence.Configurations
{
    public class MembershipPlanConfiguration :IEntityTypeConfiguration<MembershipPlan>

    {
        public void Configure(EntityTypeBuilder<MembershipPlan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(10,2)");



            builder.UsePropertyAccessMode(PropertyAccessMode.Property);
        }


    }
}
