using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Services.PaymentService;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddPaymentServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<PaymobSettings>(configuration.GetSection("PaymobSettings"));

        services.AddHttpClient<PaymobCardService>();
        services.AddHttpClient<PaymobFawryService>();

        services.AddKeyedScoped<IPaymentService, PaymobCardService>("card");
        services.AddKeyedScoped<IPaymentService, PaymobFawryService>("fawry");
    }
}
