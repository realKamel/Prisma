namespace Prisma.Application.Features.Users.Dtos;

public record RoleProfileDto(
    string Name,
    List<ProfileStatDto> Stats,
    List<ProfileActivityDto> Activities,
    List<ProfilePermissionDto>? Permissions = null);

public record ProfileStatDto(string Label, string Value, string Color);

public record ProfileActivityDto(string Message, string Time, string DotColor);

public record ProfilePermissionDto(string Name, bool Enabled);