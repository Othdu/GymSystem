using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using GymSystem.Application.Abstractions;
namespace GymSystem.Infrastructure.Persistence;

public class DbSeeder
{
    private readonly GymSystemDbContext _db;

    private readonly IPasswordHashing _hasher;
    public DbSeeder(GymSystemDbContext db, IPasswordHashing hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task SeedAsync()
    {
        // 0. a dev-only admin account
        if (!await _db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            _db.Users.Add(User.CreateAdmin("admin@gym.local", _hasher.Hash("Admin123!")));
            await _db.SaveChangesAsync();
        }
        // 1. reference data: only created once
        if (!await _db.Branches.AnyAsync())
        {
            _db.Branches.AddRange(
                new Branch("Downtown", "Cairo"),
                new Branch("Mansoura Club", "Mansoura"));

            _db.Instructors.AddRange(
                new Instructor("Sara Ahmed"),
                new Instructor("Omar Hassan"));

            _db.MembershipPlans.AddRange(
                new MembershipPlan("Monthly", PlanType.Monthly, 500m),
                new MembershipPlan("Quarterly", PlanType.Quarterly, 1300m),
                new MembershipPlan("Annual", PlanType.Annual, 4500m));

            _db.Members.AddRange(
                new Member("Ali Mostafa", "ali@example.com", "01000000001"),
                new Member("Nour Khaled", "nour@example.com", "01000000002"),
                new Member("Youssef Adel", "youssef@example.com", "01000000003"),
                new Member("Mona Samir", "mona@example.com", "01000000004"));

            await _db.SaveChangesAsync(); // SQL Server generates the Ids here
        }

        // 2. classes: recreated whenever there are no upcoming ones left
        if (!await _db.ClassSessions.AnyAsync(c => c.StartTime > DateTime.UtcNow))
        {
            var branches = await _db.Branches.OrderBy(b => b.Id).ToListAsync();
            var instructors = await _db.Instructors.OrderBy(i => i.Id).ToListAsync();

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            _db.ClassSessions.AddRange(
                new ClassSession(branches[0].Id, instructors[0].Id, "Morning Yoga",
                    tomorrow.AddHours(9), tomorrow.AddHours(10), 10),
                new ClassSession(branches[0].Id, instructors[1].Id, "Spinning",
                    tomorrow.AddHours(9).AddMinutes(30), tomorrow.AddHours(10).AddMinutes(30), 10),
                new ClassSession(branches[0].Id, instructors[1].Id, "HIIT (2 spots only)",
                    tomorrow.AddHours(18), tomorrow.AddHours(19), 2),
                new ClassSession(branches[1].Id, instructors[0].Id, "Pilates",
                    tomorrow.AddHours(11), tomorrow.AddHours(12), 8));

            await _db.SaveChangesAsync();
        }
    }
}