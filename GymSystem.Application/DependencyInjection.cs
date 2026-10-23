using FluentValidation;
using GymSystem.Application.Admin;
using GymSystem.Application.Auth;
using GymSystem.Application.Bookings;
using GymSystem.Application.Instructors;
using GymSystem.Application.Memberships;
using GymSystem.Application.Timetable;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<IAdminCatalogService, AdminCatalogService>();
        services.AddScoped<IInstructorAccountService, InstructorAccountService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddValidatorsFromAssemblyContaining<BookClassRequestValidator>();
        return services;
    }
}