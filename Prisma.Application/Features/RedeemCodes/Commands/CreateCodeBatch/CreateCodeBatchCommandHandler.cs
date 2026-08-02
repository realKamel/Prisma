using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;
namespace Prisma.Application.Features.RedeemCodes.Commands.CreateCodeBatch;

public class CreateCodeBatchCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCodeBatchCommand, Result<CreateCodeBatchResponse>>
{
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<Result<CreateCodeBatchResponse>> Handle(
        CreateCodeBatchCommand request,
        CancellationToken ct)
    {
        if (currentUser.UserId is not { } teacherId)
            return Result.Unauthorized("User is not authenticated.");

        // Verify lesson belongs to the requested academic year
        var academicYearLessonRepo = unitOfWork.GetOrCreateRepository<AcademicYearLesson, int>();
        var lessonLinked = await academicYearLessonRepo.AnyAsync(
            new AcademicYearLessonExistsSpecification(request.LessonId, request.AcademicYearId), ct);

        if (!lessonLinked)
            return Result.Error("This lesson does not belong to the selected academic year.");

        // Verify teacher is linked to the requested academic year
        var teacherAcademicYearRepo = unitOfWork.GetOrCreateRepository<AcademicYearTeacher, int>();
        var teacherHasAccess = await teacherAcademicYearRepo.AnyAsync(
            new TeacherAcademicYearExistsSpecification(teacherId, request.AcademicYearId), ct);

        if (!teacherHasAccess)
            return Result.Forbidden("You do not have access to this academic year.");

        var prefix = string.IsNullOrWhiteSpace(request.Prefix)
            ? null
            : request.Prefix.Trim().ToUpperInvariant();

        var generatedCodes = GenerateUniqueCodes(request.Count, prefix);

        var batch = new RedeemCodeEntity
        {
            LessonId = request.LessonId,
            AcademicYearId = request.AcademicYearId,
            CreatedByTeacherId = teacherId,
            Prefix = prefix,
            TotalCodes = request.Count,
            GeneratedCodes = generatedCodes
                .Select(code => new GeneratedCode { Code = code })
                .ToList(),
        };

        var batchRepo = unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>();
        batchRepo.Add(batch);
        await unitOfWork.SaveChangesAsync(ct);

        return new CreateCodeBatchResponse
        {
            BatchId = batch.Id,
            Codes = generatedCodes,
        };
    }

    private static List<string> GenerateUniqueCodes(int count, string? prefix)
    {
        var codes = new HashSet<string>(count);
        var rng = Random.Shared;

        while (codes.Count < count)
        {
            var raw = new char[8];
            for (var i = 0; i < raw.Length; i++)
                raw[i] = CodeChars[rng.Next(CodeChars.Length)];

            var body = new string(raw);
            var formatted = $"{body[..4]}-{body[4..]}";
            var code = string.IsNullOrEmpty(prefix) ? formatted : $"{prefix}-{formatted}";
            codes.Add(code);
        }

        return [.. codes];
    }
}