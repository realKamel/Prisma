using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
namespace Prisma.Application.Features.LandingPage.Queries.ExportLandingPage;

public record ExportLandingPageQuery(string email) : IRequest<Result<TeacherLandingSettings>>;