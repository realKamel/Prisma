namespace Prisma.Domain.Errors;

public static class DomainErrors
{
    public static class CommonErrors
    {
        public const string Unauthorized = "COMMON.UNAUTHORIZED";
        public const string Forbidden = "COMMON.FORBIDDEN";
        public const string BadRequest = "COMMON.BAD_REQUEST";
        public const string Conflict = "COMMON.CONFLICT";
        public const string Invalid = "COMMON.INVALID";
    }

    public static class UserErrors
    {
        public const string NotFound = "USERS.NOT_FOUND";
        public const string EmailOrPhoneInUse = "USERS.EMAIL_OR_PHONE_IN_USE";
        public const string EmailInUse = "USERS.EMAIL_IN_USE";
        public const string UnknownRole = "USERS.UNKNOWN_ROLE";
    }

    public static class StudentErrors
    {
        public const string NotFound = "STUDENTS.NOT_FOUND";
        public const string AcademicYearNotSet = "STUDENTS.ACADEMIC_YEAR_NOT_SET";
        public const string AlreadyExists = "STUDENTS.ALREADY_EXISTS";
        public const string PasswordChangeFailed = "STUDENTS.PASSWORD_CHANGE_FAILED";
    }

    public static class TeacherErrors
    {
        public const string NotFound = "TEACHERS.NOT_FOUND";
    }

    public static class AssistantErrors
    {
        public const string NotFound = "ASSISTANTS.NOT_FOUND";
    }

    public static class AdminErrors
    {
        public const string NotFound = "ADMINS.NOT_FOUND";
    }

    public static class LessonErrors
    {
        public const string NotFound = "LESSONS.NOT_FOUND";
        public const string AlreadyEnrolled = "LESSONS.ALREADY_ENROLLED";
        public const string InvalidAcademicYear = "LESSONS.INVALID_ACADEMIC_YEAR";
        public const string CannotToggleDraftedLesson = "LESSONS.CANNOT_TOGGLE_DRAFTED";
    }

    public static class LessonMaterialErrors
    {
        public const string NotFound = "LESSON_MATERIALS.NOT_FOUND";
    }

    public static class EnrollmentErrors
    {
        public const string NotFound = "ENROLLMENTS.NOT_FOUND";
    }

    public static class SectionErrors
    {
        public const string NotFound = "SECTIONS.NOT_FOUND";
    }

    public static class SectionProgressErrors
    {
        public const string NotFound = "SECTION_PROGRESS.NOT_FOUND";
    }

    public static class AssignmentErrors
    {
        public const string NotFound = "ASSIGNMENTS.NOT_FOUND";
        public const string NoAssignmentForLesson = "ASSIGNMENTS.NO_ASSIGNMENT_FOR_LESSON";
    }

    public static class AssignmentSubmissionErrors
    {
        public const string NotFound = "ASSIGNMENT_SUBMISSIONS.NOT_FOUND";
        public const string AlreadySubmitted = "ASSIGNMENT_SUBMISSIONS.ALREADY_SUBMITTED";
        public const string NotAuthorized = "ASSIGNMENT_SUBMISSIONS.NOT_AUTHORIZED";
        public const string DeadlinePassed = "ASSIGNMENT_SUBMISSIONS.DEADLINE_PASSED";
        public const string CurrentlyBeingGraded = "ASSIGNMENT_SUBMISSIONS.CURRENTLY_BEING_GRADED";
        public const string CannotReleaseOthersGradingLock = "ASSIGNMENT_SUBMISSIONS.CANNOT_RELEASE_LOCK";
        public const string ScoreExceedsMax = "ASSIGNMENT_SUBMISSIONS.SCORE_EXCEEDS_MAX";
    }

    public static class QuizErrors
    {
        public const string NotFound = "QUIZZES.NOT_FOUND";
        public const string NotAvailable = "QUIZZES.NOT_AVAILABLE";
        public const string DueDatePassed = "QUIZZES.DUE_DATE_PASSED";
        public const string LessonAlreadyHasQuiz = "QUIZZES.LESSON_ALREADY_HAS_QUIZ";
        public const string CannotDeleteWithSubmittedAttempts = "QUIZZES.CANNOT_DELETE_WITH_ATTEMPTS";
    }

    public static class QuizAttemptErrors
    {
        public const string NotFound = "QUIZ_ATTEMPTS.NOT_FOUND";
        public const string AlreadySubmitted = "QUIZ_ATTEMPTS.ALREADY_SUBMITTED";
        public const string AlreadyGraded = "QUIZ_ATTEMPTS.ALREADY_GRADED";
        public const string StillInProgress = "QUIZ_ATTEMPTS.STILL_IN_PROGRESS";
        public const string NotYetSubmitted = "QUIZ_ATTEMPTS.NOT_YET_SUBMITTED";
        public const string TimeExpired = "QUIZ_ATTEMPTS.TIME_EXPIRED";
        public const string CannotModifyAfterSubmission = "QUIZ_ATTEMPTS.CANNOT_MODIFY_AFTER_SUBMISSION";
        public const string TimeUpCannotSave = "QUIZ_ATTEMPTS.TIME_UP_CANNOT_SAVE";
        public const string AnswerNotFound = "QUIZ_ATTEMPTS.ANSWER_NOT_FOUND";
        public const string McqAnswerDoesNotNeedManualGrading = "QUIZ_ATTEMPTS.MCQ_NO_MANUAL_GRADING";
        public const string ScoreExceedsQuestionDegree = "QUIZ_ATTEMPTS.SCORE_EXCEEDS_QUESTION_DEGREE";
        public const string PenaltyExceedsStudentDegree = "QUIZ_ATTEMPTS.PENALTY_EXCEEDS_STUDENT_DEGREE";
    }

    public static class CodeBatchErrors
    {
        public const string NotFound = "CODE_BATCHES.NOT_FOUND";
        public const string LessonDoesNotBelongToAcademicYear = "CODE_BATCHES.LESSON_NOT_IN_ACADEMIC_YEAR";
        public const string NoAccessToAcademicYear = "CODE_BATCHES.NO_ACCESS_TO_ACADEMIC_YEAR";
    }

    public static class GeneratedCodeErrors
    {
        public const string NotValid = "GENERATED_CODES.NOT_VALID";
        public const string AlreadyRedeemed = "GENERATED_CODES.ALREADY_REDEEMED";
        public const string DoesNotMatchLesson = "GENERATED_CODES.DOES_NOT_MATCH_LESSON";
        public const string DoesNotMatchAcademicYear = "GENERATED_CODES.DOES_NOT_MATCH_ACADEMIC_YEAR";
    }

    public static class AuthenticationErrors
    {
        public const string RegistrationFailed = "AUTH.REGISTRATION_FAILED";
        public const string InvalidCredentials = "AUTH.INVALID_CREDENTIALS";
        public const string MustLogin = "AUTH.MUST_LOGIN";
        public const string MustProvideEmailOrPhone = "AUTH.MUST_PROVIDE_EMAIL_OR_PHONE";
        public const string InvalidPhoneNumber = "AUTH.INVALID_PHONE_NUMBER";
        public const string PhoneNumberIsRequired = "AUTH.IS_REQUIRED";
        public const string PleaseLogin = "AUTH.PLEASE_LOGIN";
        public const string InvalidResetCode = "AUTH.INVALID_RESET_CODE";
        public const string UnexpectedError = "AUTH.UNEXPECTED_ERROR";
        public const string EmailVerificationFailed = "AUTH.EMAIL_VERIFICATION_FAILED";
        public const string EmailAddressIsRequired = "AUTH.EMAIL_IS_REQUIRED";
        public const string InvalidEmailVerificationToken = "AUTH.INVALID_EMAIL_VERIFICATION_TOKEN";
    }

    public static class DomainValdtionErrors
    {
        public static class PhoneNumber
        {
            public const string Invalid = "DOMAIN.INVALID_PHONE_NUMBER";
            public const string IsRequired = "DOMAIN.PHONE_NUMBER_IS_REQUIRED";
        }
    }

    public static class ChatSessionErrors
    {
        public const string NotFound = "CHAT_SESSIONS.NOT_FOUND";
        public const string SessionNotFound = "CHAT_SESSIONS.SESSION_NOT_FOUND";
    }

    public static class StorageErrors
    {
        public const string NoFilesProvided = "STORAGE.NO_FILES_PROVIDED";
        public const string FileEmpty = "STORAGE.FILE_EMPTY";
        public const string OnlyPdfAllowed = "STORAGE.ONLY_PDF_ALLOWED";
    }

    public static class ExtractionJobErrors
    {
        public const string NotFound = "EXTRACTION_JOBS.NOT_FOUND";
    }
}