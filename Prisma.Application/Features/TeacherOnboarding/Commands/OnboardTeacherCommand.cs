using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.TeacherOnboarding.Commands;

public sealed record OnboardTeacherCommand(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string PhoneNumber,
    string Email,
    string Password,
    string Subject,
    IList<Guid> AcademicYears
) : IRequest<Result>;
