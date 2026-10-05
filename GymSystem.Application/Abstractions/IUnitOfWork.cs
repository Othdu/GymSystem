using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Application.Abstractions
{
    public   interface IUnitOfWork
    {
        Task    <int> SaveChangesAsync(CancellationToken ct=default);
    }
}
