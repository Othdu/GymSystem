using FluentValidation;
using GymSystem.Application.Bookings;
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
        services.AddValidatorsFromAssemblyContaining<BookClassRequestValidator>();
        return services;
    }
}