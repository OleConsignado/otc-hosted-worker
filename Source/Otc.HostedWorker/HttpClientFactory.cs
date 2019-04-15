using System.Net.Http;

namespace Otc.HostedWorker
{
    // TODO: Rever necessidade dessa classe. Ela foi criada para suprir a incompatibilidade
    // do HostedService com o HttpClientFactory (Otc) que depende do HttpContext (a incombitibilidade
    // ocorre entre o HttpContext e o IHostedService).
    internal class HttpClientFactory : Otc.Networking.Http.Client.Abstractions.IHttpClientFactory
    {
        public HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }

        public HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            return new HttpClient(handler);
        }

        public HttpClient CreateHttpClient(HttpClientHandler handler, bool disposeHandler)
        {
            return new HttpClient(handler, disposeHandler);
        }
    }
}
