namespace Prisma.Application.Features.RedeemCodes.Dtos;

public class GeneratedCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "used" | "available"
    public string UsedBy { get; set; } = string.Empty;
    public string UsedAt { get; set; } = string.Empty;
}