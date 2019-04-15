using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Otc.AspNetCore.ApiBoot;
using Otc.Extensions.Configuration;
using Otc.HostedWorker;
using Otc.HostedWorker.Abstractions;
using System.ComponentModel;

namespace Otc.WebHostedWorker
{
    public abstract class WebHostedWorkerStartup<THostedWorker> : ApiBootStartup
        where THostedWorker : IHostedWorker
    {
        protected WebHostedWorkerStartup(IConfiguration configuration) : base(configuration)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ConfigureApiServices(IServiceCollection services)
        {
            services.AddHostedWorker<THostedWorker>(Configuration.SafeGet<HostedWorkerConfiguration>());

            ConfigureWebHostedWorkerServices(services);
        }
        
        protected abstract void ConfigureWebHostedWorkerServices(IServiceCollection services);
    }
}
