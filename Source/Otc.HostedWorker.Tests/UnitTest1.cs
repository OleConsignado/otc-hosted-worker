using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Otc.HostedWorker.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Otc.HostedWorker.Tests
{
    public class UnitTest1
    {
        private readonly HostedWorkerConfiguration workerConfiguration;
        private readonly IHost host;

        public UnitTest1()
        {
            workerConfiguration = new HostedWorkerConfiguration();
            var builder = new HostBuilder();

            builder.ConfigureServices(services =>
            {
                services.AddHostedWorker<TestHostedWorker>(workerConfiguration);
                services.AddLogging(c =>
                {
                    c.AddDebug();
                });
            });

            host = builder.Start();

        }

        [Fact]
        public async Task Test1()
        {
            host.Services.GetService<IHostedWorkerTrigger>().Pull();
            await Task.Delay(100);
            //Assert.True(running);
            await host.StopAsync();
            //Assert.True(terminated);
        }
    }
}
