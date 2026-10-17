using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Persistence
{
    public class GymSystemDbContext : DbContext
    {
        public GymSystemDbContext(DbContextOptions<GymSystemDbContext> options) : base(options) { }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Instructor> Instructors => Set<Instructor>();
        public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
        public DbSet<Member> Members => Set<Member>();

        public DbSet<Membership> Memberships => Set<Membership>();

        public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymSystemDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

    }
}
