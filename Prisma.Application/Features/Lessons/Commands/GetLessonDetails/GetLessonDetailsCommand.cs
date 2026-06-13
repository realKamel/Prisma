using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Lessons.Commands.GetLessonDetails;

public record GetLessonDetailsCommand(int LessonId, Guid? StudentId) : IRequest<Result<LessonDetailsDto>>;
