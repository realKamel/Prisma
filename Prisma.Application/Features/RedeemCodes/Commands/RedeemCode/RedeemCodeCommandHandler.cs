using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.TeacherStudents;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;
using Prisma.Domain.Specifications.Teachers;
// Alias to avoid collision with Prisma.Application.Features.RedeemCodes namespace
using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;

public class RedeemCodeCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<RedeemCodeCommand, Result<RedeemCodeResponse>>
{
    public async Task<Result<RedeemCodeResponse>> Handle(
        RedeemCodeCommand request,
        CancellationToken ct
    )
    {
        if (currentUser.UserId is not { } studentId)
            return Result.Unauthorized("User is not authenticated.");

        // ── 1. Load the student (need AcademicYearId) ──
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepo.GetByIdAsync(studentId, ct);
        if (student is null)
            return Result.NotFound($"Student with id '{studentId}' was not found");

        if (student.AcademicYearId is null)
            return Result.Error("Student is not assigned to an academic year.");

        // ── 2. Find the GeneratedCode by code value ──
        var generatedCodeRepo = unitOfWork.GetOrCreateRepository<GeneratedCode, int>();
        var generatedCode = await generatedCodeRepo.FirstOrDefaultAsync(
            new GeneratedCodeByValueSpecification(request.Code),
            ct
        );
        if (generatedCode is null)
            return Result.Error("الكود غلط — تأكد إنك كتبته صح");

        // ── 3. Already used? ──
        if (generatedCode.RedeemedByStudentId is not null)
            return Result.Error("الكود ده اتستخدم قبل كده — لو في مشكلة تواصل مع المدرسة");

        // ── 4. Load the batch projection (lesson id, academic year, teacher id — nothing else) ──
        var batchRepo = unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>();
        var batch = await batchRepo.FirstOrDefaultAsync(
            new CodeBatchWithProjectionSpec<CodeBatchLessonInfo>(
                generatedCode.BatchId,
                b => new CodeBatchLessonInfo(b.LessonId, b.AcademicYearId, b.Lesson!.TeacherId)
            ),
            ct
        );
        if (batch is null)
            return Result.NotFound($"CodeBatch with id '{generatedCode.BatchId}' was not found");

        // ── 5. Validate lesson matches ──
        if (batch.LessonId != request.LessonId)
            return Result.Error(
                "الكود ده صح بس مش للدرس ده — تأكد إنك بتستخدم الكود الصح للدرس الصح"
            );

        // ── 6. Validate student academic year matches batch academic year ──
        if (batch.AcademicYearId != student.AcademicYearId)
            return Result.Error("الكود ده مش للسنة الدراسية بتاعتك");

        // ── 7. Check student not already enrolled ──
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var existing = await enrollmentRepo.FirstOrDefaultAsync(
            new EnrollmentByStudentAndLessonSpec(studentId, request.LessonId),
            ct
        );

        if (existing is not null && !existing.IsDeleted)
            return Result.Error("انت عندك الدرس ده بالفعل");

        // ── 8. Mark the code as redeemed ──
        generatedCode.RedeemedByStudentId = studentId;
        generatedCode.RedeemedAt = DateTimeOffset.UtcNow;

        // ── 9. Create or restore enrollment ──
        var expiresAt = DateTimeOffset.UtcNow.AddDays(30);

        Enrollment enrollment;
        if (existing is not null && existing.IsDeleted)
        {
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedBy = null;
            existing.Status = EnrollmentStatus.Active;
            existing.EnrollmentMethod = EnrollmentMethod.RedeemCode;
            existing.ExpiresAt = expiresAt;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.GeneratedCodeId = generatedCode.Id;
            enrollment = existing;
        }
        else
        {
            enrollment = new Enrollment
            {
                Status = EnrollmentStatus.Active,
                EnrollmentMethod = EnrollmentMethod.RedeemCode,
                ExpiresAt = expiresAt,
                LessonId = request.LessonId,
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

public sealed record CodeBatchLessonInfo(int LessonId, int? AcademicYearId, Guid? TeacherId);
