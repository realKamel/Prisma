using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Ardalis.Specification;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantDetailedLogs;

public class GetAssistantDetailedLogsQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService
) : IRequestHandler<GetAssistantDetailedLogsQuery, Result<GetAssistantDetailedLogsResponseDto>>
{
    public async Task<Result<GetAssistantDetailedLogsResponseDto>> Handle(
        GetAssistantDetailedLogsQuery request,
        CancellationToken cancellationToken)
    {
        // 🔒 1. التحقق من هوية اليوزر الحالي بنفس أسلوب السيستم عندكِ
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");

        var userEmail = _currentUserService.Email ?? string.Empty;

        var auditLogRepository = _unitOfWork.GetOrCreateRepository<AuditLog, int>();

        // 🌟 2. استدعاء الـ Specification الخاصة بكِ بالملي بدون أي تغيير
        var spec = new RecentAssistantLogsSpec(userEmail, request.Take);
        var logs = await auditLogRepository.ListAsync(spec, cancellationToken);

        // جلب الـ Repositories الأخرى لربط البيانات ديناميكياً
        var studentRepository = _unitOfWork.GetOrCreateRepository<Student, Guid>();
        var enrollmentRepository = _unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var logItems = new List<DetailedLogItemDto>();

        foreach (var log in logs)
        {
            string type = "view";
            string subText = log.Action ?? "عملية نظام";
            string detailText = log.TableName ?? "ملف النظام";
            string studentName = "—";
            string gradeName = "";

            // Mapping لنوع العملية بناءً على الكلمات المفتاحية في الـ Action
            if (log.Action != null)
            {
                if (log.Action.Contains("Grant", StringComparison.OrdinalIgnoreCase)) { type = "grant"; subText = "صلاحية نظام"; }
                else if (log.Action.Contains("Revoke", StringComparison.OrdinalIgnoreCase)) { type = "revoke"; subText = "إلغاء المنح"; }
                else if (log.Action.Contains("Search", StringComparison.OrdinalIgnoreCase)) { type = "search"; }
            }

            // 🌟 3. قراءة البيانات ديناميكياً بالـ Specifications الداخلية لحل مشكلة الـ Query()
            if (!string.IsNullOrEmpty(log.EntityId))
            {
                // أ- لو السجل يخص جدول الـ Enrollment (عمليات منح وإلغاء صلاحيات الدروس)
                if (log.TableName?.Equals("Enrollment", StringComparison.OrdinalIgnoreCase) == true && int.TryParse(log.EntityId, out int enrollmentId))
                {
                    var enrollmentSpec = new EnrollmentWithStudentAndLessonSpec(enrollmentId);
                    var enrollment = await enrollmentRepository.FirstOrDefaultAsync(enrollmentSpec, cancellationToken);

                    if (enrollment != null)
                    {
                        if (enrollment.Student != null)
                        {
                            studentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}".Trim();
                            if (string.IsNullOrEmpty(studentName))
                                studentName = enrollment.Student.UserName ?? "—";

                            gradeName = enrollment.Student.AcademicYear?.Title ?? "";
                        }

                        detailText = enrollment.Lesson?.Title ?? detailText;

                        if (enrollment.ExpiresAt.HasValue && type == "grant")
                        {
                            var days = (enrollment.ExpiresAt.Value - DateTimeOffset.UtcNow).Days;
                            subText = days > 0 ? $"صلاحية {days} أيام" : "صلاحية محدودة";
                        }
                    }
                }
                // ب- لو السجل يخص جدول الـ Student مباشرة (عرض أو تعديل ملف طالب)
                else if (log.TableName?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true && Guid.TryParse(log.EntityId, out Guid studentGuid))
                {
                    var studentSpec = new StudentWithAcademicYearSpec(studentGuid);
                    var student = await studentRepository.FirstOrDefaultAsync(studentSpec, cancellationToken);

                    if (student != null)
                    {
                        studentName = $"{student.FirstName} {student.LastName}".Trim();
                        if (string.IsNullOrEmpty(studentName))
                            studentName = student.UserName ?? "—";

                        detailText = "ملف الطالب";
                        subText = log.Action ?? "بيانات التسجيل";
                        gradeName = student.AcademicYear?.Title ?? "";
                    }
                }
            }

            // ⏱️ 4. حساب وقت وتاريخ العملية بأمان وتوافق مع الـ Nullable
            var logTime = log.CreatedAt?.AddHours(3) ?? DateTimeOffset.UtcNow;
            string timeString = logTime.ToString("hh:mm tt");
            string dateString = logTime.Date == DateTimeOffset.UtcNow.Date ? "اليوم" :
                                logTime.Date == DateTimeOffset.UtcNow.AddDays(-1).Date ? "أمس" :
                                logTime.ToString("yyyy-MM-dd");

            logItems.Add(new DetailedLogItemDto(
                Id: log.Id,
                Type: type,
                Detail: detailText,
                Sub: subText,
                Student: studentName,
                Grade: gradeName,
                Time: timeString,
                Date: dateString,
                Ok: true
            ));
        }

        // 📊 5. حساب الـ Meta الإحصائية ديناميكياً للـ Dashboard
        int totalThisMonth = logItems.Count;
        int grantedCount = logItems.Count(l => l.Type == "grant");
        int revokedCount = logItems.Count(l => l.Type == "revoke");
        int successCount = logItems.Count(l => l.Ok);
        int successRate = totalThisMonth > 0 ? (int)Math.Round((double)successCount / totalThisMonth * 100) : 100;

        var metaDto = new DashboardMetaDto(totalThisMonth, grantedCount, revokedCount, successRate);

        return Result<GetAssistantDetailedLogsResponseDto>.Success(
            new GetAssistantDetailedLogsResponseDto(metaDto, logItems)
        );
    }
}