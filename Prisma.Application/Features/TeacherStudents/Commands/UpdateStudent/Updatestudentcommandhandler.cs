using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;

public class UpdateStudentCommandHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    ICurrentUserService currentUserService,
    UserManager<User> userManager)
    : IRequestHandler<UpdateStudentCommand, bool>
{
    private static readonly Guid TeacherId = Guid.Parse("019ef6f7-b2b7-72e6-8ad7-5bd796c43919");

    public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var TeacherId = currentUserService.UserId ;
        var student = await studentRepo.FirstOrDefaultAsync(
            new StudentByIdForUpdateSpec(request.StudentId, TeacherId.Value),
            cancellationToken);

        if (student is null)
            throw new NotFoundException("Student", request.StudentId);

        // ── Basic fields ───────────────────────────────
        student.FirstName         = request.FirstName;
        student.SecondName        = request.SecondName;
        student.ThirdName         = request.ThirdName;
        student.LastName          = request.LastName;
        student.PhoneNumber       = request.Mobile;
        student.AcademicYearId    = request.Grade;
        student.ParentPhoneNumber = request.ParentMobile;

        // ── Email (only if changed) ────────────────────
        if (!string.Equals(student.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            // check no other user owns this email
            var existing = await identityService.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != student.Id)
                throw new BadRequestException("This email is already in use by another account.");

            student.Email    = request.Email;
            student.UserName = request.Email;         // Identity uses UserName for login
            student.NormalizedEmail    = request.Email.ToUpperInvariant();
            student.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        var updateResult = await identityService.UpdateAsync(student);
        if (!updateResult.Succeeded)
            throw new BadRequestException("Failed to update student profile.");

        // ── Password (optional — only if provided) ─────
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            // Remove old password hash then set the new one
            var removeResult = await userManager.RemovePasswordAsync(student);
            if (!removeResult.Succeeded)
                throw new BadRequestException("Failed to reset password.");

            var addResult = await userManager.AddPasswordAsync(student, request.NewPassword);
            if (!addResult.Succeeded)
                throw new BadRequestException(
                    string.Join(", ", addResult.Errors.Select(e => e.Description)));
        }

        return true;
    }
}