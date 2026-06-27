using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.AddStudent;

public class AddStudentCommandHandler(
    IIdentityService identityService,
    IUnitOfWork unitOfWork) : IRequestHandler<AddStudentCommand, Result>
{
    private static readonly Guid TeacherId = Guid.Parse("b90a811d-98a4-4353-81a5-cc75e32699b2");

    public async Task<Result> Handle(AddStudentCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await identityService.FindByEmailOrPhoneAsync(request.Email, request.Mobile);
        if (existingUser is not null)
        {
            throw new BadRequestException("Student with this email or phone already exists.");
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            ThirdName = request.ThirdName,
            LastName = request.LastName,
            PhoneNumber = request.Mobile,
            AcademicYearId = request.Grade,
            ParentPhoneNumber = request.ParentMobile,
            TeacherId = TeacherId,
            CreatedAt = DateTimeOffset.UtcNow,
            IsBlocked = false,
            IsOnline = false
        };

        var result = await identityService.CreateAsync(student, request.Password);
        if (!result.Succeeded)
        {
            throw new BadRequestException("Failed to create student. Please try again.");
        }

        await identityService.AddToRoleAsync(student, AppRoles.Student);

        return Result.Success("Student created successfully.");
    }
}
