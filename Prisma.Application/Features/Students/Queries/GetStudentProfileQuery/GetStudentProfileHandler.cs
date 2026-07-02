using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;

public class GetStudentProfileQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService
) : IRequestHandler<GetStudentProfileQuery, Result<StudentProfileDto>>
{
    public async Task<Result<StudentProfileDto>> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedException("User is not authenticated.");

        var studentRepository = _unitOfWork.GetOrCreateRepository<Student, Guid>();

        var spec = new StudentWithProfileSpec(userId.Value);
        var student = await studentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (student == null)
            throw new NotFoundException("Student", userId.Value);

        string fullName = $"{student.FirstName} {student.LastName}".Trim();

        var profileDto = new StudentProfileDto(
            Name: fullName,
            Mobile: student.PhoneNumber ?? string.Empty,
            Email: student.Email ?? string.Empty,
            Grade: student.AcademicYear?.Title ?? string.Empty, 
            Parent: student.ParentPhoneNumber ?? string.Empty  
        );

        return Result<StudentProfileDto>.Success(profileDto);
    }
}

