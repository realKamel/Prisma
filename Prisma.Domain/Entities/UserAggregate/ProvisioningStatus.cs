namespace Prisma.Domain.Entities.UserAggregate;

public enum ProvisioningStatus
{
    Pending = 1,
    Provisioning = 2,
    Active = 3,
    Failed = 4,
    Deleting = 5,
}
