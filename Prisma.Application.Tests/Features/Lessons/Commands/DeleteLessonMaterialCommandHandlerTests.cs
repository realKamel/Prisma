using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Teachers;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class DeleteLessonMaterialCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly DeleteLessonMaterialCommandHandler _sut;

    public DeleteLessonMaterialCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new DeleteLessonMaterialCommandHandler(_unitOfWork, _currentUserService, _userManager, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthorized_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new DeleteLessonMaterialCommand(1, 10);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenLessonMaterialNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        var command = new DeleteLessonMaterialCommand(1, 10);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesFileAndRemovesMaterial()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var material = new LessonMaterial { Id = 10, DownloadUrl = "materials/file.pdf" };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial> { material } };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = new DeleteLessonMaterialCommand(1, 10);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("Material deleted successfully.");

        // التأكد من حذف الملف من الخدمة السحابية
        await _storageService.Received(1).DeleteFileAsync("prisma", "materials/file.pdf", Arg.Any<CancellationToken>());

        // التأكد من حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        lesson.LessonMaterials.Should().NotContain(material);
    }
}