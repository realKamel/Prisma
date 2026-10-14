using System;
using System.Collections.Generic;
using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;

public record GetStudentPaymentHistoryQuery() : IRequest<Result<StudentPaymentHistoryResponseDto>>;

public record StudentPaymentHistoryResponseDto(
    PaymentStatsDto Stats,
    ICollection<StudentPaymentDetailsDto> Payments
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
    Guid LessonId,
    DateTimeOffset PaymentDate,
    decimal Amount,
    string Method,
    string Status
);