namespace Prisma.Domain.Exceptions;

public class StudentAcademicYearNotSetException(Guid studentId)
    : AppBaseException(
        $"Student {studentId} has no academic year assigned.",
        "STUDENT_ACADEMIC_YEAR_NOT_SET")
{
}