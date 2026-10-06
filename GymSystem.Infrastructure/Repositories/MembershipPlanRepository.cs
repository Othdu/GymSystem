using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Application.Abstractions;
using GymSystem.Domain.Entities;
using GymSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories
{
    public class MembershipPlanRepository : IMembershipPlanRepository
    {
        private readonly GymSystemDbContext _db;
        public MembershipPlanRepository(GymSystemDbContext db) => _db = db;
        public async Task<MembershipPlan?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == id, ct);
        public async Task<List<MembershipPlan>> GetAllAsync(CancellationToken ct = default) =>
            await _db.MembershipPlans.ToListAsync(ct);
       
    }
}
