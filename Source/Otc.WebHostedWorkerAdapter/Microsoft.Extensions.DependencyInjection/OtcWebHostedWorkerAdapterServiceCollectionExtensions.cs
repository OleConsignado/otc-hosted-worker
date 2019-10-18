using Otc.WebHostedWorkerAdapter;
using Otc.WebHostedWorkerAdapter.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OtcWebHostedWorkerAdapterServiceCollectionExtensions
    {
        public static IServiceCollection AddOtcWebHostedWorkerTriggerAdapter(
            this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<IWebHostedWorkerTriggerAdapterFactory, 
                WebHostedWorkerTriggerAdapterFactory>();

            return services;
        }
    }
}
