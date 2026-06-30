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
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<AddStudentCommand, Result>
{
    // private static readonly Guid TeacherId = Guid.Parse("019ef6f7-b2b7-72e6-8ad7-5bd796c43919");

    public async Task<Result> Handle(AddStudentCommand request, CancellationToken cancellationToken)
    {
        var TeacherId = currentUserService.UserId ;

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
