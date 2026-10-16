using System;
using System.Collections.Generic;
using System.Text;
using GymSystem.Application.Abstractions;

namespace GymSystem.Infrastructure.Auth
{
    public class BCryptPasswordHasher :IPasswordHashing
    {
        private const int WorkFactor = 12;
        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);



    }
}
