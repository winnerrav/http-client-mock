using Customer.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
using Customer.Api.Loggings;

namespace Customer.Api.Test.Unit
{
    public class GitHubServiceTests
    {
        private readonly IGitHubService _sut;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new ();
        private readonly Mock<ILoggerAdapter<GitHubService>> _loggerMock = new ();
        private readonly MockHttpMessageHandler _handlerMock = new MockHttpMessageHandler();

        public GitHubServiceTests()
        {
            _sut = new GitHubService(_httpClientFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task IsValidGitHubUser_ShouldLogAWarning_WhenGitHubeIsThrottled()
        {
            // Arrange

            const string githubUsername = "test";
            const string githubRateLimitMessage = "API rate limit exceeded for 127.0.0.1";
            const string githubRateLimit = $@"{{
                ""message"": ""{githubRateLimitMessage}"",
                ""documentation_url"": ""https://docks.github.com/rest/overview""
            }}";

            _loggerMock.Setup(r => r.LogWarning(
                It.IsAny<string>(), 
                It.IsAny<string>()));

            _handlerMock.When($"https://api.github.com/users/{githubUsername}")
                .Respond(HttpStatusCode.Forbidden, JsonContent.Create(new
                {
                    message = githubRateLimitMessage
                }));

            _httpClientFactoryMock.Setup(x => x.CreateClient("GitHub"))
                .Returns(new HttpClient(_handlerMock)
                {
                    BaseAddress = new Uri("https://api.github.com")
                });

            //Act
            var resultAction = () => _sut.IsValidGitHubUser(githubUsername);

            // Assert
            await resultAction
                .Should()
                .ThrowAsync<HttpRequestException>()
                .WithMessage(githubRateLimitMessage);

            _loggerMock.Verify(x =>
                x.LogWarning(It.Is<string>(s => s == "Request throttled with message: {Message}"),
                It.Is<string>(s => s == githubRateLimitMessage)));
        }
    }
}