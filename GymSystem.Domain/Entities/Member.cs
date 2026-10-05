using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Domain.Entities
{
    public class Member
    {
        public int Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;

        private Member() { }
        public Member(string fullName, string email, string phone)
        {
            FullName = fullName; Email = email; Phone = phone;
        }
    }
}
