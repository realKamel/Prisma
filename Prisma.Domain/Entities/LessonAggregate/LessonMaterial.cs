using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.ObjectLibraryAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class LessonMaterial : BaseEntity
{
    // public string Title { get; set; } = string.Empty;
    // public string Size { get; set; } = string.Empty;
    public LessonMaterialType Type { get; set; }

    // public string DownloadUrl { get; set; } = string.Empty;
    public FileObject FileObject { get; set; }
    public Guid FileObjectId { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public static Result<LessonMaterial> Create(Guid fileObjectId, LessonMaterialType type)
    {
        if (fileObjectId == Guid.Empty)
        {
            return Result.Invalid(
                new ValidationError(nameof(FileObjectId), "File object ID is required.")
            );
        }

        return new LessonMaterial() { FileObjectId = fileObjectId, Type = type };
    }

    internal void AttachTo(Lesson lesson)
    {
        Lesson = lesson;
    }
}
