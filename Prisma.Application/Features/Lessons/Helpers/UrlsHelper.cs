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

    public static string ConstructLessonAssignmentObjectKey(Guid workspaceId, Guid lessonId)
    {
        // return $"{workspaceId}/lessons/{lessonId}/assignments/{Guid.CreateVersion7()}";

        return ConstructObjectKey(
            workspaceId.ToString(),
            "lessons",
            lessonId.ToString(),
            "assignments",
            Guid.CreateVersion7().ToString()
        );
    }
}
