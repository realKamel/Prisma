namespace Prisma.Domain.Entities;

public class Assistant : User
{
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}