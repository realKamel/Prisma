namespace Prisma.Application.Common.Constants;

public static class ErrorKeys
{
    public static class Common
    {
        public const string SystemError = "COMMON.SYSTEM_ERROR";
        public const string BadRequest = "COMMON.BAD_REQUEST";
        public const string Unauthorized = "COMMON.UNAUTHORIZED";
        public const string Forbidden = "COMMON.FORBIDDEN";
        public const string NotFound = "COMMON.NOT_FOUND";
        public const string Conflict = "COMMON.CONFLICT";
        public const string ValidationFailed = "COMMON.VALIDATION_FAILED";
    }

    public static class Auth
    {
        public const string InvalidCredentials = "AUTH.INVALID_CREDENTIALS";
        public const string TokenExpired = "AUTH.TOKEN_EXPIRED";
        public const string AccountLocked = "AUTH.ACCOUNT_LOCKED";
    }

    public static class User
    {
        public const string NotFound = "USER.NOT_FOUND";
        public const string EmailExists = "USER.EMAIL_EXISTS";
        public const string PhoneExists = "USER.PHONE_EXISTS";
    }
}
