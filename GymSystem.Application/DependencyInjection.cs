using FluentValidation;
using GymSystem.Application.Bookings;
using GymSystem.Application.Memberships;
using GymSystem.Application.Timetable;
using Microsoft.Extensions.DependencyInjection;
using GymSystem.Application.Auth;
using GymSystem.Application.Instructors;


namespace GymSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddValidatorsFromAssemblyContaining<BookClassRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}