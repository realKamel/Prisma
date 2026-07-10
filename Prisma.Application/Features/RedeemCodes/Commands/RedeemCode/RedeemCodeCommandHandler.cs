using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;
using Prisma.Application.Features.TeacherStudents;

// Alias to avoid collision with Prisma.Application.Features.RedeemCodes namespace
using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;

internal class RedeemCodeCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<RedeemCodeCommand, Result<RedeemCodeResponse>>
{
    public async Task<Result<RedeemCodeResponse>> Handle(
        RedeemCodeCommand request,
        CancellationToken ct)
    {
        if (currentUser.UserId is not { } studentId)
            throw new UnauthorizedException("User is not authenticated.");

        // ── 1. Load the student (need AcademicYearId) ──
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepo.GetByIdAsync(studentId, ct)
                      ?? throw new NotFoundException("Student", studentId);

        if (student.AcademicYearId is null)
            throw new BadRequestException("Student is not assigned to an academic year.");

        // ── 2. Find the GeneratedCode by code value ──
        var generatedCodeRepo = unitOfWork.GetOrCreateRepository<GeneratedCode, int>();
        var generatedCode = await generatedCodeRepo.FirstOrDefaultAsync(
                                new GeneratedCodeByValueSpecification(request.Code), ct)
                            ?? throw new BadRequestException("الكود غلط — تأكد إنك كتبته صح");

        // ── 3. Already used? ──
        if (generatedCode.RedeemedByStudentId is not null)
            throw new BadRequestException("الكود ده اتستخدم قبل كده — لو في مشكلة تواصل مع المدرسة");

        // ── 4. Load the batch to check lesson + academic year ──
        var batchRepo = unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>();
        var batch = await batchRepo.FirstOrDefaultAsync(
                        new CodeBatchWithLessonSpecification(generatedCode.BatchId), ct)
                    ?? throw new NotFoundException("CodeBatch", generatedCode.BatchId);

        // ── 5. Validate lesson matches ──
        if (batch.LessonId != request.LessonId)
            throw new BadRequestException("الكود ده صح بس مش للدرس ده — تأكد إنك بتستخدم الكود الصح للدرس الصح");

        // ── 6. Validate student academic year matches batch academic year ──
        if (batch.AcademicYearId != student.AcademicYearId)
            throw new BadRequestException("الكود ده مش للسنة الدراسية بتاعتك");

        // ── 7. Check student not already enrolled ──
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var existing = await enrollmentRepo.FirstOrDefaultAsync(
            new EnrollmentByStudentAndLessonSpec(studentId, request.LessonId), ct);

        if (existing is not null && !existing.IsDeleted)
            throw new BadRequestException("انت عندك الدرس ده بالفعل");

        // ── 8. Mark the code as redeemed ──
        generatedCode.RedeemedByStudentId = studentId;
        generatedCode.RedeemedAt = DateTimeOffset.UtcNow;
        // unitOfWork.DbContext.Entry(generatedCode).State = EntityState.Modified;

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
            // unitOfWork.DbContext.Entry(existing).State = EntityState.Modified;
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

        await unitOfWork.SaveChangesAsync(ct);

        return new RedeemCodeResponse
        {
            EnrollmentId = enrollment.Id,
            ExpiresAt = expiresAt,
        };
    }
}