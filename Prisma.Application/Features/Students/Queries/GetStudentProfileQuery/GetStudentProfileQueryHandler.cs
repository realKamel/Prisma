using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;

public class GetStudentProfileQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService
) : IRequestHandler<GetStudentProfileQuery, Result<StudentProfileDto>>
{
    public async Task<Result<StudentProfileDto>> Handle(GetStudentProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result.Unauthorized("User is not authenticated.");

        var studentRepository = _unitOfWork.GetOrCreateRepository<Student, Guid>();

        var spec = new StudentWithProfileSpec(userId.Value);
        var student = await studentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (student == null)
            return Result.NotFound($"Student with id '{userId.Value}' was not found");


        var profileDto = new StudentProfileDto(
            FirstName: student.FirstName ?? string.Empty,
            SecondName: student.SecondName ?? string.Empty,
            ThirdName: student.ThirdName ?? string.Empty,
            LastName: student.LastName ?? string.Empty,
            Mobile: student.PhoneNumber ?? string.Empty,
            Email: student.Email ?? string.Empty,
            Grade: student.AcademicYearId ?? 0,
            ParentMobile: student.ParentPhoneNumber ?? string.Empty
        );

        return Result<StudentProfileDto>.Success(profileDto);
    }
}