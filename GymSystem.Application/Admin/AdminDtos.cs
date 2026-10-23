using FluentValidation;
using GymSystem.Domain.Enums;

namespace GymSystem.Application.Admin;

public record CreateBranchRequest(string Name, string City);
public record BranchResponse(int Id, string Name, string City);

public record CreatePlanRequest(string Name, PlanType Type, decimal Price);
public record PlanResponse(int Id, string Name, PlanType Type, decimal Price);

public record CreateClassRequest(
    int BranchId, int InstructorId, string Title,
    DateTime StartTime, DateTime EndTime, int Capacity);
public record ClassResponse(
    int Id, int BranchId, int InstructorId, string Title,
    DateTime StartTime, DateTime EndTime, int Capacity);

public record CreateInstructorAccountRequest(string FullName, string Email, string Password);
public record InstructorAccountResponse(int UserId, int InstructorId, string FullName, string Email);

public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}

public class CreatePlanRequestValidator : AbstractValidator<CreatePlanRequest>
{
    public CreatePlanRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(100_000);
    }
}

public class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.InstructorId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 200);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
    }
}

public class CreateInstructorAccountRequestValidator : AbstractValidator<CreateInstructorAccountRequest>
{
    public CreateInstructorAccountRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(72)
            .Matches("[A-Za-z]").WithMessage("Password must contain a letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");
    }
}