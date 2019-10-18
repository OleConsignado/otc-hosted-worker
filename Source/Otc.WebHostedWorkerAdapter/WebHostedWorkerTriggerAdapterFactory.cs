using Microsoft.Extensions.Logging;
using Otc.Networking.Http.Client.Abstractions;
using Otc.WebHostedWorkerAdapter.Abstractions;
using System;

namespace Otc.WebHostedWorkerAdapter
{
    public class WebHostedWorkerTriggerAdapterFactory : IWebHostedWorkerTriggerAdapterFactory
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IHttpClientFactory httpClientFactory;

        public WebHostedWorkerTriggerAdapterFactory(ILoggerFactory loggerFactory, 
            IHttpClientFactory httpClientFactory)
        {
            this.loggerFactory = loggerFactory ??
                throw new System.ArgumentNullException(nameof(loggerFactory));
            this.httpClientFactory = httpClientFactory ??
                throw new System.ArgumentNullException(nameof(httpClientFactory));
        }

        public IWebHostedWorkerTriggerAdapter Create(string baseUrl)
        {
            Uri baseUri;

            try
            {
                baseUri = new Uri(baseUrl);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Value must be a valid Uri.", nameof(baseUrl), e);
            }

            return new WebHostedWorkerTriggerAdapter(baseUri, loggerFactory, httpClientFactory);
        }
    }
}
