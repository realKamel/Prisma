using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.API.Filters;

namespace Prisma.API.Common;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[TranslateResultToActionResult]
[LocalizeResult]
[EnableRateLimiting(RateLimitPolicies.UserRead)]
public class ApiController : ControllerBase { }
