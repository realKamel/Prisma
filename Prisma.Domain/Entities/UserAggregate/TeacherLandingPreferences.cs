namespace Prisma.Domain.Entities.UserAggregate;

public class TeacherLandingSettings
{
    public NavLogo navLogo { get; set; }
    public Hero hero { get; set; }
    public MiniQuiz miniQuiz { get; set; }
    public List<Review> reviews { get; set; }
}

public class Badge
{
    public string icon { get; set; }
    public string text { get; set; }
}

public class Hero
{
    public string tag { get; set; }
    public string title { get; set; }
    public string subtitle { get; set; }
    public string ctaPrimary { get; set; }
    public string ctaSecondary { get; set; }
    public string teacherImage { get; set; }
    public List<Badge> badges { get; set; }
}

public class MiniQuiz
{
    public int id { get; set; }
    public string question { get; set; }
    public List<Option> options { get; set; }
    public string correct { get; set; }
}

public class NavLogo
{
    public string teacherName { get; set; }
    public string platformName { get; set; }
    public string logoLetter { get; set; }
}

public class Option
{
    public string id { get; set; }
    public string label { get; set; }
}

public class Review
{
    public string stars { get; set; }
    public string body { get; set; }
    public string avatar { get; set; }
    public string name { get; set; }
    public string role { get; set; }
}

