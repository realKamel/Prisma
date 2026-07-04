using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;

public record GetStudentPaymentHistoryQuery() : IRequest<Result<StudentPaymentHistoryResponseDto>>;

public record StudentPaymentHistoryResponseDto(
    PaymentStatsDto Stats,
    List<StudentPaymentDetailsDto> Payments
);

public record PaymentStatsDto(
    decimal TotalAmount,
    int LessonsPurchased,
    int ActiveLessons,
    int ExpiredLessons
);

public record StudentPaymentDetailsDto(
    string Id,
    string LessonTitle,
    int LessonId,
    string PosterVariant,
    DateTimeOffset PaymentDate,
    decimal Amount,
    string Method,
    string Status
);