using UglyToad.PdfPig;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Services;

public class PdfTextExtractor : IPdfTextExtractor
{
    public Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var text = new System.Text.StringBuilder();

        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }
        }

        return Task.FromResult(text.ToString());
    }
}
