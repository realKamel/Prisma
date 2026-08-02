using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Prisma.API.Common;

[ApiController]
[Route("api/v1/[controller]")]
[TranslateResultToActionResult]
public class ApiController : ControllerBase
{
}