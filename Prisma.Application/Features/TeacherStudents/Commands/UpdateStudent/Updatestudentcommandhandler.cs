using MediatR;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;

public class UpdateStudentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStudentCommand, bool>
{
    private static readonly Guid TeacherId = Guid.Parse("b90a811d-98a4-4353-81a5-cc75e32699b2");

    public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var student = await studentRepo.FirstOrDefaultAsync(
            new StudentByIdForUpdateSpec(request.StudentId, TeacherId),
            cancellationToken);

        if (student is null)
            throw new NotFoundException("Student", request.StudentId);

        student.FirstName   = request.FirstName;
        student.SecondName  = request.SecondName;
        student.ThirdName   = request.ThirdName;
        student.LastName    = request.LastName;
        student.PhoneNumber = request.Mobile;
        student.AcademicYearId   = request.Grade;
        student.ParentPhoneNumber = request.ParentMobile;

        studentRepo.Update(student);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}