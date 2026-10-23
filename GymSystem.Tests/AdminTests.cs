using GymSystem.Application.Abstractions;
using GymSystem.Application.Admin;
using GymSystem.Application.Common;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Enums;
using GymSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace GymSystem.Tests;

public class AdminCatalogServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBranchRepository> _branches = new();
    private readonly Mock<IInstructorRepository> _instructors = new();
    private readonly Mock<IMembershipPlanRepository> _plans = new();
    private readonly Mock<IClassSessionRepository> _sessions = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly AdminCatalogService _service;

    public AdminCatalogServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(Now);
        _branches.Setup(b => b.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _instructors.Setup(i => i.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sessions.Setup(s => s.InstructorHasOverlapAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _service = new AdminCatalogService(
            _branches.Object, _instructors.Object, _plans.Object, _sessions.Object, _uow.Object, _clock.Object);
    }

    private static CreateClassRequest ClassRequest(DateTime? start = null)
    {
        var s = start ?? Now.AddDays(1);
        return new CreateClassRequest(1, 1, "Yoga", s, s.AddHours(1), 10);
    }

    [Fact]
    public async Task CreateClass_Valid_SavesOnce()
    {
        ClassSession? added = null;
        _sessions.Setup(s => s.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()))
                 .Callback<ClassSession, CancellationToken>((c, _) => added = c)
                 .Returns(Task.CompletedTask);

        var response = await _service.CreateClassAsync(ClassRequest());

        Assert.NotNull(added);
        Assert.Equal("Yoga", response.Title);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClass_InThePast_Throws()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _service.CreateClassAsync(ClassRequest(start: Now.AddHours(-1))));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClass_UnknownBranch_ThrowsNotFound()
    {
        _branches.Setup(b => b.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateClassAsync(ClassRequest()));
    }

    [Fact]
    public async Task CreateClass_UnknownInstructor_ThrowsNotFound()
    {
        _instructors.Setup(i => i.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateClassAsync(ClassRequest()));
    }

    [Fact]
    public async Task CreateClass_WhenInstructorHasOverlap_ThrowsAndDoesNotSave()
    {
        _sessions.Setup(s => s.InstructorHasOverlapAsync(
                1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateClassAsync(ClassRequest()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class InstructorAccountServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHashing> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly InstructorAccountService _service;

    public InstructorAccountServiceTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
        _service = new InstructorAccountService(_users.Object, _hasher.Object, _uow.Object);
    }

    private static CreateInstructorAccountRequest Request() =>
        new("Mariam Tarek", "Mariam@Gym.local", "Passw0rd123");

    [Fact]
    public async Task Create_WhenEmailExists_ThrowsAndDoesNotSave()
    {
        _users.Setup(u => u.EmailExistsAsync("mariam@gym.local", It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(Request()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithNewEmail_StoresHashedInstructorLogin_AndSavesOnce()
    {
        User? saved = null;
        _users.Setup(u => u.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        _users.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .Callback<User, CancellationToken>((user, _) => saved = user)
              .Returns(Task.CompletedTask);

        var response = await _service.CreateAsync(Request());

        Assert.NotNull(saved);
        Assert.Equal(UserRole.Instructor, saved!.Role);
        Assert.Equal("hashed-password", saved.PasswordHash);
        Assert.Equal("mariam@gym.local", saved.Email);
        Assert.Equal("Mariam Tarek", response.FullName);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}