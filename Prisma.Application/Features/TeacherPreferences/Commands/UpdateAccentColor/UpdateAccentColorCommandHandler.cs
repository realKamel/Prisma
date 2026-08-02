using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teacher;

namespace Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;

public sealed class UpdateAccentColorCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateAccentColorCommand, Result>
{
    public async Task<Result> Handle(UpdateAccentColorCommand request, CancellationToken ct)
    {
        if (currentUserService.UserId is not { } teacherId)
        {
            return Result.Error("المستخدم غير مصرح له");
        }

        var repository = unitOfWork.GetOrCreateRepository<Prisma.Domain.Entities.TeacherPreferences, Guid>();
        var spec = new TeacherPreferencesByTeacherIdSpecification(teacherId);
        var preferences = await repository.FirstOrDefaultAsync(spec, ct);

        if (preferences is null)
        {
            preferences = Prisma.Domain.Entities.TeacherPreferences.CreateDefault(teacherId);
            preferences.UpdateAccentColor(request.AccentColor);
            repository.Add(preferences);
        }
        else
        {
            preferences.UpdateAccentColor(request.AccentColor);
            repository.Update(preferences);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.SuccessWithMessage("تم حفظ اللون بنجاح");
    }
}