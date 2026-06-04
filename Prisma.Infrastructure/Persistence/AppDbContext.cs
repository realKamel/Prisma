using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence;


public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Assistant> Assistants { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
    public DbSet<AttemptAnswer> AttemptAnswers { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<Code> Codes { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LessonQuiz> LessonQuizzes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<QuestionLessonQuiz> QuestionLessonQuizzes { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<SectionProgress> SectionProgresses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Question> Questions { get; set; }






    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // to apply all configuration for class that implements IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}