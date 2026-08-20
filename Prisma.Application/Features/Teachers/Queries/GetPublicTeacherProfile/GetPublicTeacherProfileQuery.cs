using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

namespace Prisma.Application.Features.Teachers.Queries.GetPublicTeacherProfile;

public record GetPublicTeacherProfileQuery(Guid Id)
    : IRequest<Result<PublicTeacherProfileResponse>>;

public record PublicTeacherProfileResponse(
    Guid Id,
    string FirstName,
    string SecondName,
    string Subject,
    string? Bio,
    string? ImageUrl,
    int LessonsCount,
    /** Total students who follow / subscribe to this teacher. */
    int TotalStudents,
    /** Years of teaching experience. */
    int? YearsOfExperience,
    IReadOnlyList<AcademicYearDto> AcademicYears,
    bool Featured = false
);
