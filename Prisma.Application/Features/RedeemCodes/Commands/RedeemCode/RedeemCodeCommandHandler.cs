using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.TeacherStudents;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.RedeemCodes;
using Prisma.Domain.Specifications.Teachers;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;
// Alias to avoid collision with Prisma.Application.Features.RedeemCodes namespace
using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;

internal sealed class RedeemCodeCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser
) : IRequestHandler<RedeemCodeCommand, Result<RedeemCodeResponse>>
{
    public async Task<Result<RedeemCodeResponse>> Handle(
        RedeemCodeCommand request,
        CancellationToken ct
    )
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Unauthorized();
        }

        // ── 1. Load the student (need AcademicYearId) ──
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var student = await studentRepo.GetByIdAsync(studentId, ct);

        if (student is null)
        {
            return Result.NotFound($"Student with id '{studentId}' was not found");
        }

        if (student.AcademicYearId is null)
        {
            return Result.Invalid(
                new ValidationError(
                    "AcademicYearId",
                    "Student is not assigned to an academic year.",
                    "REDEEM_CODE.NOT_ACADEMIC_ID",
                    ValidationSeverity.Error
                )
            );
        }

        var generatedCodeRepo = unitOfWork.GetOrCreateRepository<GeneratedCode, int>();

        var generatedCode = await generatedCodeRepo.FirstOrDefaultAsync(
            new GeneratedCodeByValueSpecification(request.Code),
            ct
        );
        if (generatedCode is null)
        {
            var error = new ValidationError(
                "Code",
                "Code is Invalid",
                "REDEEM_CODE.NOT_ACADEMIC_ID",
                ValidationSeverity.Error
            );
            return Result.Invalid(error);
        }

        if (generatedCode.RedeemedByStudentId is not null)
            return Result.Error("الكود ده اتستخدم قبل كده — لو في مشكلة تواصل مع المدرسة");

        var batchRepo = unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>();

        var batch = await batchRepo.FirstOrDefaultAsync(
            new CodeBatchWithProjectionSpec<CodeBatchLessonInfo>(
                generatedCode.BatchId,
                b => new CodeBatchLessonInfo(b.Lesson.PublicId, b.AcademicYearId, b.Lesson!.TeacherId)
            ),
            ct
        );

        if (batch is null)
        {
            return Result.NotFound($"CodeBatch with id '{generatedCode.BatchId}' was not found");
        }

        if (batch.LessonId != request.LessonId)
        {
            return Result.Error(
                "الكود ده صح بس مش للدرس ده — تأكد إنك بتستخدم الكود الصح للدرس الصح"
            );
        }

        if (batch.AcademicYearId != student.AcademicYearId)
        {
            return Result.Error("الكود ده مش للسنة الدراسية بتاعتك");
        }

        // ── 7. Check student not already enrolled ──
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var existing = await enrollmentRepo.FirstOrDefaultAsync(
            new EnrollmentByStudentAndLessonSpec(studentId, request.LessonId),
            ct
        );

        if (existing is not null && !existing.IsDeleted)
        {
            return Result.Conflict("انت عندك الدرس ده بالفعل");
        }

        var lesson = await lessonRepo.FirstOrDefaultAsync(
            new LessonWithProjectionSpec<LessonInfo>(
                request.LessonId, l => new LessonInfo(l.Id, l.ValidityDuration.Seconds)));
        generatedCode.RedeemedByStudentId = studentId;
        generatedCode.RedeemedAt = DateTimeOffset.UtcNow;

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(lesson.validityDuration);

        Enrollment enrollment = default!;
        if (existing is not null && existing.IsDeleted)
        {
            existing.Status = EnrollmentStatus.Active;
            existing.EnrollmentMethod = EnrollmentMethod.RedeemCode;
            // existing.ExpiresAt = expiresAt;
            enrollment.ValidityDateRange = DateRange.StartingNow(expiresAt);
            existing.GeneratedCodeId = generatedCode.Id;
            enrollment = existing;
        }
        else
        {
            enrollment = new Enrollment
            {
                Status = EnrollmentStatus.Active,
                EnrollmentMethod = EnrollmentMethod.RedeemCode,
                // ExpiresAt = expiresAt,
                ValidityDateRange = DateRange.StartingNow(expiresAt),
                LessonId = lesson.Id,
                StudentId = studentId,
                GeneratedCodeId = generatedCode.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            enrollmentRepo.Add(enrollment);
        }

        // ── 10. Ensure teacher-student pairing exists ──
        var teacherStudentRepo = unitOfWork.GetOrCreateRepository<TeacherStudent, int>();
        var pairExists = await teacherStudentRepo.AnyAsync(
            new TeacherStudentPairSpec(batch.TeacherId.Value, studentId),
            ct
        );

        if (!pairExists)
        {
            teacherStudentRepo.Add(
                new TeacherStudent { TeacherId = batch.TeacherId.Value, StudentId = studentId }
            );
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new RedeemCodeResponse { EnrollmentId = enrollment.Id, ExpiresAt = expiresAt };
    }
}

public sealed record LessonInfo(int Id, long validityDuration);

public sealed record CodeBatchLessonInfo(Guid LessonId, int? AcademicYearId, Guid? TeacherId);
