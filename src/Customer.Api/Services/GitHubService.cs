using Customer.Api.Loggings;
using System.Text.Json.Nodes;

namespace Customer.Api.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter<GitHubService> _logger;


        public GitHubService(IHttpClientFactory httpClientFactory, 
            ILoggerAdapter<GitHubService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> IsValidGitHubUser(string username)
        {
            var client = _httpClientFactory.CreateClient("GitHub");
            var response = await client.GetAsync($"/users/{username}");
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
                var message = responseBody!["message"]!.ToString();

                _logger.LogWarning("Request throttled with message: {Message}", message);
                throw new HttpRequestException(message);
            }

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
