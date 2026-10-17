using GymSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Application.Abstractions
{
    public interface IClassSessionRepository
    {
        Task<ClassSession?> GetByIdWithBookingsAsync(int id, CancellationToken ct = default);
        Task<List<ClassSession>> GetMemberSessionsAsync(
            int memberId, DateTime from, DateTime to, int excludeSessionId, CancellationToken ct = default);
        Task<List<ClassSession>> GetByBranchAndDateAsync(int branchId, DateOnly date, CancellationToken ct = default);
        Task<List<ClassSession>> GetByInstructorAndDateAsync(int instructorId, DateOnly date, CancellationToken ct = default);
        Task AddAsync(ClassSession session, CancellationToken ct = default);
        Task<bool> InstructorHasOverLapAsync(int instructorId, DateTime start, DateTime end, CancellationToken ct = default);
    }
}


