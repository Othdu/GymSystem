using GymSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymSystem.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

        Task AddAsync(User user, CancellationToken ct = default);


    }
}
