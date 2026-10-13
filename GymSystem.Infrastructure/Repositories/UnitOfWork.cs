using GymSystem.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using Microsoft.EntityFrameworkCore;

using GymSystem.Infrastructure.Persistence;

namespace GymSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GymSystemDbContext _db;

        public UnitOfWork(GymSystemDbContext db) => _db = db;
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("This class was just changed by someone else. Please try again.");
            }
        }



    }
}
