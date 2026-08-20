using Ardalis.Result;
using MediatR;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Commands.ActivateTeacherCommand;

public class ActivateTeacherCommandHandler : IRequestHandler<ActivateTeacherCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateTeacherCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        ActivateTeacherCommand request,
        CancellationToken cancellationToken
    )
    {
        var teacherRepo = _unitOfWork.GetOrCreateRepository<Teacher, Guid>();
        var spec = new TeacherByIdSpecification(request.TeacherId);

        var teacher = await teacherRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (teacher is null)
        {
            return Result<bool>.NotFound($"المعلم برقم {request.TeacherId} غير موجود");
        }

        teacher.Status = TeacherStatus.Active;
        teacher.SuspensionReason = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
