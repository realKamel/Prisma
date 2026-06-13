namespace Prisma.Application.Features.LessonCatalog.Queries;

public class LessonCatalogDto
{
    public int      Id                { get; init; }
    public string?  Title             { get; init; }
    public decimal  Price             { get; init; }
    public string   Status            { get; init; } = string.Empty;
    public string?  PrerequisiteLabel { get; init; }
    public string?  ExpiredDate       { get; init; }
    public string   TeacherName       { get; init; } = string.Empty;
    public string   TeacherInitial    { get; init; } = string.Empty;
    public string   Subject           { get; init; } = string.Empty;
    public int      DurationHours     { get; init; }
    public string?  ImageUrl          { get; init; }
    public string   Currency          { get; init; } = "جنيه";
}