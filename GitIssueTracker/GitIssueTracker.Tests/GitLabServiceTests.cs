using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitIssueTracker.Core.Enums;
using GitIssueTracker.Core.Models;
using GitIssueTracker.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace GitIssueTracker.Tests
{
    public class GitLabServiceTests
    {
        [Fact]
        public async Task CreateIssueAsync_ShouldReturnValidResponse_WhenApiSucceeds()
        {
            // ARRANGE
            var json = "{\"iid\": 101, \"web_url\": \"https://gitlab.com/repo/issues/101\", \"state\": \"opened\"}";
            var response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json)
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var client = new HttpClient(handlerMock.Object);
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GitServices:GitLab:Token"]).Returns("fake-token");
            var loggerMock = new Mock<ILogger<GitLabService>>();

            var service = new GitLabService(client, configMock.Object, loggerMock.Object);

            var request = new IssueRequest
            {
                Title = "New GitLab Issue",
                Description = "Created from test"
            };

            // ACT
            var result = await service.CreateIssueAsync("user/repo", request);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(101, result.IssueNumber);
            Assert.Equal("https://gitlab.com/repo/issues/101", result.Url);
            Assert.Equal(IssueStatus.Open, result.Status);
        }

        [Fact]
        public async Task UpdateIssueAsync_ShouldReturnUpdatedResponse_WhenApiSucceeds()
        {
            // ARRANGE
            var json = "{\"iid\": 101, \"web_url\": \"https://gitlab.com/repo/issues/101\", \"state\": \"opened\"}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var client = new HttpClient(handlerMock.Object);
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GitServices:GitLab:Token"]).Returns("fake-token");
            var loggerMock = new Mock<ILogger<GitLabService>>();

            var service = new GitLabService(client, configMock.Object, loggerMock.Object);

            var request = new IssueRequest
            {
                Title = "Updated Title",
                Description = "Updated Description"
            };

            // ACT
            var result = await service.UpdateIssueAsync("user/repo", 101, request);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(101, result.IssueNumber);
            Assert.Equal("https://gitlab.com/repo/issues/101", result.Url);
            Assert.Equal(IssueStatus.Open, result.Status);
        }

        [Fact]
        public async Task CloseIssueAsync_ShouldReturnTrue_WhenApiSucceeds()
        {
            // ARRANGE
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var client = new HttpClient(handlerMock.Object);
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GitServices:GitLab:Token"]).Returns("fake-token");
            var loggerMock = new Mock<ILogger<GitLabService>>();

            var service = new GitLabService(client, configMock.Object, loggerMock.Object);

            // ACT
            var result = await service.CloseIssueAsync("user/repo", 101);

            // ASSERT
            Assert.True(result);
        }
    }
}
