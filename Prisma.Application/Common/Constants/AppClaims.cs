using System.Reflection;

namespace Prisma.Application.Common.Constants;

public static class AppClaims
{
    public const string PermissionsClaim = "permissions";

    public static class Permissions
    {
        //Lesson & Content Management
        public const string CreateLesson = "lesson:create";
        public const string UpdateLesson = "lesson:update";
        public const string DeleteLesson = "lesson:delete";

        public const string ManageResources = "resource:manage"; // Uploading PDFs, links, files

        //Question Bank & Assessments 
        public const string ManageQuestionBank = "questions:manage"; // Reusing questions across exams
        public const string CreateAssessment = "assessment:create";
        public const string UpdateAssessment = "assessment:update";
        public const string DeleteAssessment = "assessment:delete";

        // Grading & Evaluation 
        public const string GradeAssessment = "grading:assessment";
        public const string GradeExam = "grading:exam";
        public const string OverrideGrade = "grading:override"; // Overwrite existing grades
        public const string PublishGrades = "grading:publish"; // Make grades visible to students
        public const string ViewGradeBook = "gradebook:view"; // See the entire class grade matrix

        //Enrollment Management 
        public const string EnrollStudent = "student:enroll";
        public const string UnenrollStudent = "student:unenroll";
        public const string ViewStudentProfile = "student:view_profile";

        //  Analytics & Reporting 
        public const string ViewSystemAnalytics = "analytics:system"; // System-wide metrics
        public const string ViewLessonAnalytics = "analytics:lesson";

        public static IReadOnlySet<string> All { get; } = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet();

        public static bool IsValid(string permission) => All.Contains(permission);
        public static bool AreValid(IEnumerable<string> permissions) => permissions.All(IsValid);
    }

    public static class Policies
    {
        // Content Creators / Teacher
        public const string CanManageContent = nameof(CanManageContent);
        public const string CanManageAssessments = nameof(CanManageAssessments);

        // Evaluators (Teacher, TAs)
        public const string CanEvaluateStudents = nameof(CanEvaluateStudents);

        // Administration / Teacher
        public const string CanManageEnrollments = nameof(CanManageEnrollments);
        public const string CanViewReports = nameof(CanViewReports);

        public static IReadOnlyDictionary<string, IReadOnlyList<string>> PermissionMap { get; } =
            new Dictionary<string, IReadOnlyList<string>>
            {
                [CanManageContent] =
                [
                    Permissions.CreateLesson,
                    Permissions.UpdateLesson,
                    Permissions.DeleteLesson,
                    Permissions.ManageResources,
                ],
                [CanManageAssessments] =
                [
                    Permissions.ManageQuestionBank,
                    Permissions.CreateAssessment,
                    Permissions.UpdateAssessment,
                    Permissions.DeleteAssessment,
                ],
                [CanEvaluateStudents] =
                [
                    Permissions.GradeAssessment,
                    Permissions.GradeExam,
                    Permissions.OverrideGrade,
                    Permissions.PublishGrades,
                    Permissions.ViewGradeBook,
                ],
                [CanManageEnrollments] =
                [
                    Permissions.EnrollStudent,
                    Permissions.UnenrollStudent,
                    Permissions.ViewStudentProfile,
                ],
                [CanViewReports] =
                [
                    Permissions.ViewSystemAnalytics,
                    Permissions.ViewLessonAnalytics,
                ],
            };
    }
}