using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Filters;

namespace Prisma.API.Common;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[TranslateResultToActionResult]
[LocalizeResult]
public class ApiController : ControllerBase
{
}