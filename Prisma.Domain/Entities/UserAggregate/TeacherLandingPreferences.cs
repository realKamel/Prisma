namespace Prisma.Domain.Entities.UserAggregate;

public class TeacherLandingSettings
{
    public NavLogo NavLogo { get; set; }
    public Hero Hero { get; set; }
    public MiniQuiz MiniQuiz { get; set; }
    public List<Review> Reviews { get; set; }
}

public record Badge
{
    public string Icon { get; set; }
    public string Text { get; set; }
}

public record Hero
{
    public string Tag { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string CtaPrimary { get; set; }
    public string CtaSecondary { get; set; }
    public string TeacherImage { get; set; }
    public List<Badge> Badges { get; set; }
}

public record MiniQuiz
{
    public int Id { get; set; }
    public string Question { get; set; }
    public List<Option> Options { get; set; }
    public string Correct { get; set; }
}

public record NavLogo
{
    public string TeacherName { get; set; }
    public string PlatformName { get; set; }
    public string LogoLetter { get; set; }
}

public record Option
{
    public string Id { get; set; }
    public string Label { get; set; }
}

public record Review
{
    public string Stars { get; set; }
    public string Body { get; set; }
    public string Avatar { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
}