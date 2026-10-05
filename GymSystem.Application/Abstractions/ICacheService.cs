using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Application.Abstractions
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
        Task RemoveAsync(string key, CancellationToken ct = default);

      
    }
}
