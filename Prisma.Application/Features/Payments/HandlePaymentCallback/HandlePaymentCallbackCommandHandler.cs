using MediatR;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Payments;
using Prisma.Domain.Specifications.Teacher;

namespace Prisma.Application.Features.Payments.HandleCallback;

public class HandlePaymentCallbackCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<HandlePaymentCallbackCommand>
{
    public async Task Handle(HandlePaymentCallbackCommand request, CancellationToken ct)
    {
        if (!request.Success) return;

        var paymentRepo = unitOfWork.GetOrCreateRepository<Payment, int>();
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var teacherStudentRepo = unitOfWork.GetOrCreateRepository<TeacherStudent, int>();

        var payment = await paymentRepo.FirstOrDefaultAsync(new PaymentByProviderRefSpec(request.OrderId), ct);
        if (payment is null) return;

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTimeOffset.UtcNow;

        enrollmentRepo.Add(new Enrollment
        {
            StudentId = payment.StudentId,
            LessonId = payment.LessonId,
            PaymentId = payment.Id,
            EnrollmentMethod = EnrollmentMethod.OnlinePayment,
            Status = EnrollmentStatus.Active,
        });

        var teacherId = await lessonRepo.FirstOrDefaultAsync(
            new LessonWithProjectionSpec<Guid?>(payment.LessonId, l => (Guid?)l.TeacherId), ct);

        if (teacherId is not null)
        {
            var pairExists = await teacherStudentRepo.AnyAsync(
                new TeacherStudentPairSpec(teacherId.Value, payment.StudentId), ct);

            if (!pairExists)
            {
                teacherStudentRepo.Add(new TeacherStudent
                {
                    TeacherId = teacherId.Value,
                    StudentId = payment.StudentId
                });
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}