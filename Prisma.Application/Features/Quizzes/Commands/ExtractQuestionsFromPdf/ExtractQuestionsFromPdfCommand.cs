using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Quizzes.Commands.ExtractQuestionsFromPdf;

public record ExtractQuestionsFromPdfCommand(
    string FileName,
    string FilePath
) : IRequest<Result<ExtractionJobDto>>;
