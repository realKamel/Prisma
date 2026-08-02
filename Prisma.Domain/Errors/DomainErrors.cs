namespace Prisma.Domain.Errors;

public static class DomainErrors
{
    public static class CommonErrors
    {
        public static string Unauthorized(string message = "You are not authenticated.")
            => message;

        public static string Forbidden(string message = "You are not authorized to perform this action.")
            => message;

        public static string BadRequest(string message)
            => message;

        public static string Conflict(string message)
            => message;

        public static string Invalid(string message)
            => message;
    }

    public static class UserErrors
    {
        public static string NotFound(string userId)
            => $"User with id '{userId}' was not found.";

        public static string EmailOrPhoneInUse(string emailOrPhone)
            => $"A user with this email or phone '{emailOrPhone}' already exists.";

        public static string EmailInUse(string email)
            => $"This email '{email}' is already in use by another account.";

        public static string UnknownRole(string role)
            => $"Unknown role '{role}'.";
    }

    public static class StudentErrors
    {
        public static string NotFound(Guid id)
            => $"Student with id '{id}' was not found.";

        public static string AcademicYearNotSet(Guid studentId)
            => $"Student {studentId} has no academic year assigned.";

        public static string AlreadyExists(string emailOrPhone)
            => "Student with this email or phone already exists.";

        public static string PasswordChangeFailed(string details)
            => $"فشلت عملية تغيير كلمة المرور: {details}";
    }

    public static class TeacherErrors
    {
        public static string NotFound(Guid id)
            => $"Teacher with id '{id}' was not found.";

        public static string NotFound(string email)
            => $"Teacher with email '{email}' was not found.";
    }

    public static class AssistantErrors
    {
        public static string NotFound(Guid id)
            => $"Assistant with id '{id}' was not found.";
    }

    public static class AdminErrorErrors
    {
        public static string NotFound(Guid id)
            => $"Admin with id '{id}' was not found.";
    }

    public static class LessonErrors
    {
        public static string NotFound(int id)
            => $"Lesson with id '{id}' was not found.";

        public static string AlreadyEnrolled
            => "Student is already enrolled in this lesson.";

        public static string InvalidAcademicYear
            => "Invalid academic year.";

        public static string CannotToggleDraftedLesson
            => "Cannot toggle status for a drafted lesson.";
    }

    public static class LessonMaterialErrors
    {
        public static string NotFound(int materialId)
            => $"Lesson material with id '{materialId}' was not found.";
    }

    public static class EnrollmentErrors
    {
        public static string NotFound(string studentId, int lessonId)
            => $"Enrollment for student '{studentId}' and lesson '{lessonId}' was not found.";
    }

    public static class SectionErrors
    {
        public static string NotFound(int id)
            => $"Section with id '{id}' was not found.";
    }

    public static class SectionProgressErrors
    {
        public static string NotFound(int sectionId)
            => $"Section progress for section '{sectionId}' was not found.";
    }

    public static class AssignmentErrors
    {
        public static string NotFound(int lessonId)
            => $"Assignment for lesson '{lessonId}' was not found.";

        public static string NoAssignmentForLesson
            => "لا يوجد واجب لهذا الدرس";
    }

    public static class AssignmentSubmissionErrors
    {
        public static string NotFound(int lessonId)
            => $"Assignment submission for lesson '{lessonId}' was not found.";

        public static string AlreadySubmitted
            => "لقد سلمت هذا الواجب مسبقاً";

        public static string NotAuthorized
            => "غير مصرح لك بتسليم هذا الواجب";

        public static string DeadlinePassed
            => "انتهى الموعد النهائي للتسليم";

        public static string CurrentlyBeingGraded
            => "التسليم ده بيتصحح دلوقتي من شخص تاني";

        public static string CannotReleaseOthersGradingLock
            => "مينفعش تفكي قفل تصحيح شخص تاني";

        public static string ScoreExceedsMax(decimal score, decimal max)
            => $"الدرجة ({score}) أكبر من الدرجة الكاملة ({max})";
    }

    public static class QuizErrors
    {
        public static string NotFound(int id)
            => "الاختبار غير موجود";

        public static string NotAvailable
            => "الاختبار غير متاح حاليًا";

        public static string DueDatePassed
            => "انتهى موعد هذا الاختبار";

        public static string LessonAlreadyHasQuiz
            => "الحصة دي عندها اختبار بالفعل";

        public static string CannotDeleteWithSubmittedAttempts
            => "مينفعش تحذف/ي اختبار عنده محاولات مسلمة أو متصححة";
    }

    public static class QuizAttemptErrors
    {
        public static string NotFound
            => "المحاولة غير موجودة";

        public static string AlreadySubmitted
            => "تم تسليم هذا الاختبار من قبل";

        public static string AlreadyGraded
            => "المحاولة دي متصححة بالفعل";

        public static string StillInProgress
            => "الطالب لسه في الاختبار";

        public static string NotYetSubmitted
            => "لم يتم تسليم هذا الاختبار بعد";

        public static string TimeExpired
            => "انتهى وقت هذه المحاولة";

        public static string CannotModifyAfterSubmission
            => "لا يمكن تعديل الإجابات بعد التسليم";

        public static string TimeUpCannotSave
            => "انتهى وقت الاختبار، لا يمكن حفظ المزيد من الإجابات";

        public static string AnswerNotFound(int answerId)
            => $"الإجابة رقم {answerId} غير موجودة في هذه المحاولة";

        public static string McqAnswerDoesNotNeedManualGrading
            => $"الإجابة MCQ ومش محتاجة تصحيح يدوي";

        public static string ScoreExceedsQuestionDegree(decimal degree)
            => $"الدرجة المدخلة أكبر من الدرجة الكاملة للسؤال ({degree})";

        public static string PenaltyExceedsStudentDegree(decimal penalty, decimal degree)
            => $"الخصم ({penalty}) أكبر من درجة الطالب الحالية ({degree})";
    }

    public static class CodeBatchErrors
    {
        public static string NotFound(int id)
            => $"CodeBatch with id '{id}' was not found.";

        public static string LessonDoesNotBelongToAcademicYear
            => "This lesson does not belong to the selected academic year.";

        public static string NoAccessToAcademicYear
            => "You do not have access to this academic year.";
    }

    public static class GeneratedCodeErrors
    {
        public static string NotValid
            => "الكود غلط — تأكد إنك كتبته صح";

        public static string AlreadyRedeemed
            => "الكود ده اتستخدم قبل كده — لو في مشكلة تواصل مع المدرسة";

        public static string DoesNotMatchLesson
            => "الكود ده صح بس مش للدرس ده — تأكد إنك بتستخدم الكود الصح للدرس الصح";

        public static string DoesNotMatchAcademicYear
            => "الكود ده مش للسنة الدراسية بتاعتك";
    }

    public static class AuthenticationErrors
    {
        public static string RegistrationFailed
            => "Registration Failed";

        public static string InvalidCredentials
            => "Invalid credentials";

        public static string MustLogin
            => "Login First";

        public static string PleaseLogin
            => "Please Login";

        public static string InvalidResetCode
            => "Code Invalid";

        public static string UnexpectedError
            => "something went wrong";

        public static string EmailVerificationFailed
            => "Something Went Wrong";

        public static string InvalidEmailVerificationToken
            => "Invalid token.";
    }

    public static class ChatSessionErrors
    {
        public static string NotFound(Guid id)
            => $"ChatSession with id '{id}' was not found.";

        public static string SessionNotFound
            => "Session not found";
    }

    public static class StorageErrors
    {
        public static string NoFilesProvided
            => "No files provided for upload.";

        public static string FileEmpty
            => "لم يتم رفع أي ملف";

        public static string OnlyPdfAllowed
            => "يسمح فقط بملفات PDF";
    }

    public static class ExtractionJob
    {
        public static string NotFound
            => "لم يتم العثور على المهمة";
    }
}