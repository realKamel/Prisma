using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;

public record UpdateStudentProfileCommand(
    string FirstName,
    string secondName,
    string thirdName,
    string LastName,
    string Mobile,
    int AcademicYearId, 
    string ParentMobile
) : IRequest<Result<bool>>;