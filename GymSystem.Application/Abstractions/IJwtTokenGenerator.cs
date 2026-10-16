using System;
using System.Collections.Generic;
using System.Text;
using GymSystem.Domain.Entities;
namespace GymSystem.Application.Abstractions
{

    public record TokenResult(string Token, DateTime ExpiresAtUtc);
    public interface IJwtTokenGenerator
    {
        TokenResult GenerateToken(User user);


    }
}
