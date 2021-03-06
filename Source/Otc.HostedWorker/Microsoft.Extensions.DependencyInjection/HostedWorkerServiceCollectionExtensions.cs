﻿using Otc.HostedWorker;
using Otc.HostedWorker.Abstractions;
using Otc.Networking.Http.Client.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HostedWorkerServiceCollectionExtensions
    {
        public static IServiceCollection AddHostedWorker(this IServiceCollection services, Type hostedWorkerImplementationType, HostedWorkerConfiguration hostedWorkerConfiguration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (hostedWorkerImplementationType == null)
            {
                throw new ArgumentNullException(nameof(hostedWorkerImplementationType));
            }

            if (hostedWorkerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(hostedWorkerConfiguration));
            }

            if (!typeof(IHostedWorker).IsAssignableFrom(hostedWorkerImplementationType))
            {
                throw new InvalidOperationException($"{nameof(hostedWorkerImplementationType)} param must implements {nameof(IHostedWorker)} interface.");
            }

            services.AddSingleton(hostedWorkerConfiguration);
            services.AddHostedService<BackgroundHostedService>();
            services.AddSingleton<IHostedWorkerHealth, HostedWorkerHealth>();
            services.AddSingleton<IHostedWorkerTrigger, HostedWorkerTrigger>();
            services.AddSingleton<IHttpClientFactory>(new HttpClientFactory());
            services.AddScoped(typeof(IHostedWorker), hostedWorkerImplementationType);

            return services;
        }

        public static IServiceCollection AddHostedWorker<THostedWorkerImpl>(this IServiceCollection services, HostedWorkerConfiguration hostedWorkerConfiguration)
            where THostedWorkerImpl : IHostedWorker
        {
            return AddHostedWorker(services, typeof(THostedWorkerImpl), hostedWorkerConfiguration);
        }
    }
}
