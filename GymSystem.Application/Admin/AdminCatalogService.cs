using GymSystem.Application.Abstractions;
using GymSystem.Application.Common;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Application.Admin;

public interface IAdminCatalogService
{
    Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request, CancellationToken ct = default);
    Task<ClassResponse> CreateClassAsync(CreateClassRequest request, CancellationToken ct = default);
}

public class AdminCatalogService : IAdminCatalogService
{
    private readonly IBranchRepository _branches;
    private readonly IInstructorRepository _instructors;
    private readonly IMembershipPlanRepository _plans;
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _clock;

    public AdminCatalogService(
        IBranchRepository branches,
        IInstructorRepository instructors,
        IMembershipPlanRepository plans,
        IClassSessionRepository sessions,
        IUnitOfWork uow,
        IDateTimeProvider clock)
    {
        _branches = branches;
        _instructors = instructors;
        _plans = plans;
        _sessions = sessions;
        _uow = uow;
        _clock = clock;
    }

    public async Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        var branch = new Branch(request.Name.Trim(), request.City.Trim());
        await _branches.AddAsync(branch, ct);
        await _uow.SaveChangesAsync(ct);

        return new BranchResponse(branch.Id, branch.Name, branch.City);
    }

    public async Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request, CancellationToken ct = default)
    {
        var plan = new MembershipPlan(request.Name.Trim(), request.Type, request.Price); // domain re-checks name and price
        await _plans.AddAsync(plan, ct);
        await _uow.SaveChangesAsync(ct);

        return new PlanResponse(plan.Id, plan.Name, plan.Type, plan.Price);
    }

    public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request, CancellationToken ct = default)
    {
        if (!await _branches.ExistsAsync(request.BranchId, ct))
            throw new NotFoundException($"Branch {request.BranchId} was not found.");

        if (!await _instructors.ExistsAsync(request.InstructorId, ct))
            throw new NotFoundException($"Instructor {request.InstructorId} was not found.");

        if (request.StartTime <= _clock.UtcNow)
            throw new DomainException("A class must start in the future.");

        if (await _sessions.InstructorHasOverlapAsync(request.InstructorId, request.StartTime, request.EndTime, ct))
            throw new DomainException("This instructor already teaches another class at that time.");

        var session = new ClassSession(
            request.BranchId, request.InstructorId, request.Title.Trim(),
            request.StartTime, request.EndTime, request.Capacity);

        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return new ClassResponse(
            session.Id, session.BranchId, session.InstructorId, session.Title,
            session.StartTime, session.EndTime, session.Capacity);
    }
}