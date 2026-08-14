using MediatR;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Features.Teachers.Commands.SuspendTeacherCommand;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teacher;

namespace Application.Features.Teachers.Commands.SuspendTeacher;

using Ardalis.Result;

public class SuspendTeacherCommandHandler : IRequestHandler<SuspendTeacherCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SuspendTeacherCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(SuspendTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacherRepo = _unitOfWork.GetOrCreateRepository<Prisma.Domain.Entities.UserAggregate.Teacher, Guid>();
        var spec = new TeacherByIdSpecification(request.TeacherId);

        var teacher = await teacherRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (teacher is null)
        {
            return Result.NotFound($"المعلم برقم {request.TeacherId} غير موجود");
        }

        teacher.Status = TeacherStatus.Suspended;
        teacher.SuspensionReason = request.Reason;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}