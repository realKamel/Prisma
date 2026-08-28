using Microsoft.Extensions.Localization;
using Prisma.Application.Abstractions.Services;

namespace Prisma.API.Localization;

public class AppLocalizer(IStringLocalizer<SharedResources> localizer) : IAppLocalizer
{
    public string this[string name] => localizer[name];

    public string this[string name, params object[] arguments] => localizer[name, arguments];
}
