namespace Prisma.Application.Common.Constants;

public static class AppClaims
{
    public const string PermissionsClaim = "permissions";

    public static class Permissions
    {
        public const string ManageCourses = "manage_courses";
        public const string ViewAnalytics = "view_analytics";
        public const string ManageUsers = "manage_users";
        public const string ManageStudents = "manage_Students";
        public const string SubmitAssignment = "submit_assignment";
    }

    public static class Policies
    {
        public const string CanManageCourses = "CanManageCourses";
        public const string CanViewAnalytics = "CanViewAnalytics";
        public const string CanManageUsers = "CanManageUsers";
        public const string CanSubmitAssignment = "CanSubmitAssignment";
    }
}