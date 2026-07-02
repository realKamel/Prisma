using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;

public class UpdateStudentProfileCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager
) : IRequestHandler<UpdateStudentProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedException("User is not authenticated.");

        var studentRepository = _unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (student == null)
            throw new NotFoundException("Student", userId.Value);

        student.FirstName = request.FirstName.Trim();
        student.SecondName = request.secondName.Trim();
        student.ThirdName = request.thirdName.Trim();
        student.LastName = request.LastName.Trim();
        student.PhoneNumber = request.Mobile.Trim();
        student.AcademicYearId = request.AcademicYearId;
        student.ParentPhoneNumber = request.ParentMobile.Trim();

     
        studentRepository.Update(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}