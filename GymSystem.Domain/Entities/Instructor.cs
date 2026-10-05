using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Domain.Entities
{
    public class Instructor
    {
        public int Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;

        private Instructor() { }
        public Instructor(string fullName) { FullName = fullName; }
    }

}
