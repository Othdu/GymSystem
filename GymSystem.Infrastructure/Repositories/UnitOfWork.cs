using GymSystem.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.Application.Abstractions;
using GymSystem.Infrastructure.Persistence;

namespace GymSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GymSystemDbContext _db;

        public UnitOfWork(GymSystemDbContext db) => _db = db;
        public Task <int >SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);



    }
}
