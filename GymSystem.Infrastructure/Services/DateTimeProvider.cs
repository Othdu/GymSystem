using GymSystem.Application.Abstractions;

namespace GymSystem.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}