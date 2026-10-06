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
    public class ClassSessionConfiguration: IEntityTypeConfiguration<ClassSession>
    {
        public void Configure(EntityTypeBuilder<ClassSession> builder)
        {
            builder.HasKey(cs => cs.Id);
            builder.Property(cs => cs.StartTime).IsRequired().HasMaxLength(100);
            
            builder .Property<uint>("RowVersion").IsRowVersion();

            builder.Metadata.FindNavigation(nameof(ClassSession.Bookings))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);   

            builder.HasIndex(cs => new { cs.BranchId, cs.StartTime });
        }
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(b => new { b.ClassSessionId, b.MemberId });
    }
}