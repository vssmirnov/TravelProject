using System.Net;
using System.Text;
using Newtonsoft.Json;
using TestApp.Models;

namespace TestApp.Services
{
    public class ProviderOneSearchService : IProviderOneSearchService
    {
        private readonly HttpClient _httpClient;

        public ProviderOneSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ProviderOneSearchResponse> SearchAsync(ProviderOneSearchRequest request, CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://provider-one/api/v1/search");
            var json = JsonConvert.SerializeObject(request);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ProviderOneSearchResponse>(responseJson);

            return response;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "http://provider-one/api/v1/ping");

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
