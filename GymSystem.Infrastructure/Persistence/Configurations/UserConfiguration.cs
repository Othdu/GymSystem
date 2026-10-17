using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Member)
               .WithOne()
               .HasForeignKey<User>(u => u.MemberId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Instructor)
               .WithOne()
               .HasForeignKey<User>(u => u.InstructorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}