using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Quizzes.Commands.ExtractQuestionsFromPdf;

public record ExtractQuestionsFromPdfCommand(
    string FileName,
    string FilePath
) : IRequest<Result<ExtractionJobDto>>;
