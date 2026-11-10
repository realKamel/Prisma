namespace Prisma.Application.Features.Lessons.Helpers;

public static class UrlsHelper
{
    public static string ConstructObjectKey(params string[] parts)
    {
        return string.Join("/", parts);
    }

    public static string ConstructLessonCoverObjectKey(Guid workspaceId, Guid lessonId)
    {
        return ConstructObjectKey(
            workspaceId.ToString(),
            "lessons",
            lessonId.ToString(),
            Guid.CreateVersion7().ToString()
        );
    }

    public static string ConstructLessonAssignmentObjectKey(Guid lessonId, Guid objectKey)
    {
        // return $"lessons/{lessonId}/assignments/{Guid.CreateVersion7()}";

        return ConstructObjectKey(
            "lessons",
            lessonId.ToString(),
            "assignments",
            objectKey.ToString()
        );
    }

    public static string ConstructLessonMaterialsObjectKey(Guid lessonId, Guid objectId)
    {
        // return $"lessons/{lessonId}/Materials/{Guid.CreateVersion7()}";

        return ConstructObjectKey("lessons", lessonId.ToString(), "materials", objectId.ToString());
    }
}
