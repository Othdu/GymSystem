using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Domain.Entities
{
    public class Branch
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;

        private Branch() { }
        public Branch(string name, string city) { Name = name; City = city; }
    }

}
