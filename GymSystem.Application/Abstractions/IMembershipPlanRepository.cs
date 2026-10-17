using GymSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Application.Abstractions
{
    public interface IMembershipPlanRepository
    {
        Task<MembershipPlan?> GetByIdAsync(int id, CancellationToken ct = default); 
        Task<List<MembershipPlan>> GetAllAsync(CancellationToken ct = default);

        Task AddAsync(MembershipPlan plan, CancellationToken ct = default);

    }
}
