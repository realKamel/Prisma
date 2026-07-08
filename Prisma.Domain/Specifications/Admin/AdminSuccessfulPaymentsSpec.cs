using System;
using global::Prisma.Domain.Entities.PaymentAggregate;
using global::Prisma.Domain.Enums;
using Ardalis.Specification; // أو حسب مكتبة الـ Specification اللي عندك

namespace Prisma.Domain.Specifications.AdminDashboard;

public sealed class AdminSuccessfulPaymentsSpec : Specification<Payment>
{
    public AdminSuccessfulPaymentsSpec()
    {
        Query
            .Where(p => p.Status == PaymentStatus.Completed)
            .Include(p => p.Student)
            .Include(p => p.Lesson);
    }
}