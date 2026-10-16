using System;
using System.Collections.Generic;
using System.Text;

namespace GymSystem.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string Secret { get; init; } = string.Empty;
        public int ExpiryMinutes { get; init; } = 60;

    }
}
