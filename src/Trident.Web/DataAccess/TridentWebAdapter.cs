using RestSharp;
using System.Threading.Tasks;

namespace Trident.Web.DataAccess
{
    public interface ITridentWebAdapter
    {
        Task<string> GetWebResponseAsync(string url);
    }

    public class TridentWebAdapter
    {
        public async Task<string> GetWebResponseAsync(string url)
        {
            using (var client = new RestClient())
            {
                var request = new RestRequest(url, Method.Get);

                request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:159.0) Gecko/20100101 Firefox/159.0");
                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                var response = await client.GetAsync(request);

                if (response.IsSuccessful == false)
                {
                    throw new System.Exception($"Unable to get a response from {url}, the response was {response.StatusCode}{response.ErrorException}");
                }

                return response.Content;
            }
        }
    }
}
