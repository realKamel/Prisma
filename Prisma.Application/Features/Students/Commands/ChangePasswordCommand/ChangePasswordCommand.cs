using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Commands.ChangePasswordCommand;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result<bool>>;
