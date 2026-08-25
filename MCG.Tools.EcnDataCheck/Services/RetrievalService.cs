using MCG.Tools.EcnDataCheck.Interfaces;
using MCG.Tools.EcnDataCheck.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MCG.Tools.EcnDataCheck.Services
{
    public class RetrievalService : IRetrievalService
    {
        private readonly HttpClient _httpClient;

        public RetrievalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<RetrievalResponse> SearchAsync(string query)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                "Token a remplacer");

            var body = new
            {
                queryString = query,
                dataSource = "sharePoint"
            };

            var response = await _httpClient.PostAsJsonAsync("https://graph.microsoft.com/v1.0/copilot/retrieval", body);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RetrievalResponse>();
        }
    }
}
