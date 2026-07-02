using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;

public record GetStudentProfileQuery() : IRequest<Result<StudentProfileDto>>;

public record StudentProfileDto(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string Grade,
    string Parent
);