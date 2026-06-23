using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Quizzes;

public class StudentsByAcademicYearSpecification : Specification<Student>
{
    public StudentsByAcademicYearSpecification(int academicYearId)
    {
        Query.Where(s => s.AcademicYearId == academicYearId);
    }

}
