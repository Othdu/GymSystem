using System;
using System.Collections.Generic;
using System.Text;

namespace GymSystem.Application.Common
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
    }
}
