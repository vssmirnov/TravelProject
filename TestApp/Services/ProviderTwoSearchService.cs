using System.Net;
using System.Text;
using Newtonsoft.Json;
using TestApp.Models;

namespace TestApp.Services
{
    public class ProviderTwoSearchService : IProviderTwoSearchService
    {
        private readonly HttpClient _httpClient;

        public ProviderTwoSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ProviderTwoSearchResponse> SearchAsync(ProviderTwoSearchRequest request, CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://provider-two/api/v1/search");
            var json = JsonConvert.SerializeObject(request);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ProviderTwoSearchResponse>(responseJson);

            return response;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "http://provider-two/api/v1/ping");

            try
            {
                var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }
    }
}
