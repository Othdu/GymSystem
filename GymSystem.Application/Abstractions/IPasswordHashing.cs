using System;
using System.Collections.Generic;
using System.Text;

namespace GymSystem.Application.Abstractions
{
    public  interface IPasswordHashing
    {
        string Hash(string password);
        bool Verify(string password, string hash);

    }
}
