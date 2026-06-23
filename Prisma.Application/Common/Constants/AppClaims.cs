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
        public const string PublishGrades = "grading:publish";  // Make grades visible to students
        public const string ViewGradebook = "gradebook:view";   // See the entire class grade matrix

        //Enrollment Management 
        public const string EnrollStudent = "student:enroll";
        public const string UnenrollStudent = "student:unenroll";
        public const string ViewStudentProfile = "student:view_profile";

        //  Analytics & Reporting 
        public const string ViewSystemAnalytics = "analytics:system"; // System-wide metrics
        public const string ViewLessonAnalytics = "analytics:lesson";

    }
    public static class Policies
    {
        // Content Creators / Teacher
        public const string CanManageContent = "CanManageContent";
        public const string CanManageAssessments = "CanManageAssessments";

        // Evaluators (Teacher, TAs)
        public const string CanEvaluateStudents = "CanEvaluateStudents";

        // Administration / Registrars
        public const string CanManageEnrollments = "CanManageEnrollments";
        public const string CanViewReports = "CanViewReports";
    }

    public static class Roles
    {
        public const string Student = "Student";
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Assistant = "Assistant";
    }

    public static class Cookies
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
    }
}