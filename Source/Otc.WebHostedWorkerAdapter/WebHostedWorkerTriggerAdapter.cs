using Microsoft.Extensions.Logging;
using Otc.Networking.Http.Client.Abstractions;
using Otc.WebHostedWorkerAdapter.Abstractions;
using System;
using System.Threading.Tasks;

namespace Otc.WebHostedWorkerAdapter
{
    internal class WebHostedWorkerTriggerAdapter : IWebHostedWorkerTriggerAdapter
    {
        private readonly Uri baseUri;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger logger;
        public const int RequestWaitTimeoutMilliseconds = 1000;

        public WebHostedWorkerTriggerAdapter(Uri baseUri,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory)
        {
            logger = loggerFactory?
                .CreateLogger($"{typeof(WebHostedWorkerTriggerAdapter).FullName}-{baseUri}")
                ?? throw new ArgumentNullException(nameof(loggerFactory));

            this.baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            this.httpClientFactory = httpClientFactory ?? 
                throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public void Pull()
        {
            // Call and wait for RequestWaitTimeoutMilliseconds
            // Forget Exceptions (the only way to track exceptions is on log).

            var task = Task.Run(async () =>
            {
                try
                {
                    await RequestTriggerPullAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception ocurred while trying to request " +
                        "WebHostedWorker/Trigger/Pull.");
                }
            });

            if (Task.WaitAny(new Task[] { task }, RequestWaitTimeoutMilliseconds) == -1)
            {
                logger.LogWarning("Call to WebHostedWorker/Trigger/Pull took longer than " +
                    "{RequestWaitTimeoutMilliseconds} milliseconds to reply.",
                    RequestWaitTimeoutMilliseconds);
            }
        }

        private async Task RequestTriggerPullAsync()
        {
            using (var httpClient = httpClientFactory.CreateHttpClient())
            {
                httpClient.BaseAddress = baseUri;
                await httpClient.PostAsync("v1/Trigger/Pull", null);
            }
        }
    }
}
