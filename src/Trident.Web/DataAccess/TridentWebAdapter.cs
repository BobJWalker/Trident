using RestSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Trident.Web.DataAccess
{
    public interface ITridentWebAdapter
    {
        Task<string> GetWebResponseAsync(string url);
    }

    public class TridentWebAdapter : ITridentWebAdapter
    {
        private static HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                UseCookies = true,                 // keep any cookies the site sets
                CookieContainer = new CookieContainer()
            };

            var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

            var headers = client.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/130.0.0.0");
            headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml");
            headers.AcceptLanguage.ParseAdd("en-US,en");
            headers.Add("Upgrade-Insecure-Requests", "1");

            return client;
        }

        public async Task<string> GetWebResponseAsync(string url)
        {
            using (var client = CreateClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(
                            $"Unable to get a response from {url}, the response was {(int)response.StatusCode} {response.ReasonPhrase}");
                    }

                    return await response.Content.ReadAsStringAsync();
                }                   
            }                
        }
    }
}
