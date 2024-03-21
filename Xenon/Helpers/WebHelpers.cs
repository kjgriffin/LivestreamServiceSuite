using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xenon.Helpers
{
    public class WebHelpers
    {
        public static HttpClient httpClient
        {
            get
            {
                return XenonHttpClientBuilder.GetFactory().CreateClient();
            }
        }

        public static async Task<string> DownloadText(string url)
        {
            var resp = await httpClient.GetAsync(url);
            var text = await resp.Content.ReadAsStringAsync();

            return text;
        }

    }

    static class XenonHttpClientBuilder
    {
        public const string XENON_HTTP_CLIENT = "XenonClientBuilder";
        internal static IServiceProvider _services { get; private set; }

        internal static IHttpClientFactory GetFactory()
        {
            if (_services == null)
            {
                ConfigureAndBuild();
            }
            var factory = _services.GetService<IHttpClientFactory>();
            return factory;
        }

        internal static void ConfigureAndBuild()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddHttpClient(XENON_HTTP_CLIENT)
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(GetRetryPolicy());

            _services = services.BuildServiceProvider();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                                       .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                                       .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
        }

    }
}
