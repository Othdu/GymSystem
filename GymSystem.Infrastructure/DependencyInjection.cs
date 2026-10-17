using GymSystem.Application.Abstractions;
using GymSystem.Infrastructure.Persistence;
using GymSystem.Infrastructure.Repositories;
using GymSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GymSystem.Infrastructure.Auth;
namespace GymSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<GymSystemDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IClassSessionRepository, ClassSessionRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        var jwt = new JwtSettings
        {
            Issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing."),
            Audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing."),
            Secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is missing. Set it with dotnet user-secrets."),
            ExpiryMinutes = int.TryParse(config["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 60
        };

        if (jwt.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");

        services.AddSingleton(jwt);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHashing, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
