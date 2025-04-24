using System.Net;
using System.Net.Http;
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
    public class GitHubServiceTests
    {
        [Fact]
        public async Task CreateIssueAsync_ShouldReturnValidResponse_WhenApiCallSucceeds()
        {
            // ARRANGE
            var json = "{\"number\":1,\"html_url\":\"https://github.com/user/repo/issues/1\",\"state\":\"open\"}";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new System.Uri("https://api.github.com/")
            };

            var loggerMock = new Mock<ILogger<GitHubService>>();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GitServices:GitHub:Token"]).Returns("fake-token");

            var service = new GitHubService(client, configMock.Object, loggerMock.Object);

            var request = new IssueRequest
            {
                Title = "Test Issue",
                Description = "Test Desc"
            };

            // ACT
            var result = await service.CreateIssueAsync("user/repo", request);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.IssueNumber);
            Assert.Equal("https://github.com/user/repo/issues/1", result.Url);
            Assert.Equal(IssueStatus.Open, result.Status);
        }

        [Fact]
        public async Task UpdateIssueAsync_ShouldReturnUpdatedResponse_WhenApiCallSucceeds()
        {
            // Arrange
            var json = "{\"number\":1,\"html_url\":\"https://github.com/user/repo/issues/1\",\"state\":\"open\"}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GitServices:GitHub:Token"]).Returns("fake-token");

            var loggerMock = new Mock<ILogger<GitHubService>>();

            var service = new GitHubService(client, configMock.Object, loggerMock.Object);

            var request = new IssueRequest
            {
                Title = "Updated title",
                Description = "Updated description"
            };

            // Act
            var result = await service.UpdateIssueAsync("user/repo", 1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.IssueNumber);
            Assert.Equal("https://github.com/user/repo/issues/1", result.Url);
            Assert.Equal(IssueStatus.Open, result.Status);
        }

        [Fact]
        public async Task CloseIssueAsync_ShouldReturnTrue_WhenApiCallSucceeds()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"state\": \"closed\"}")
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GitServices:GitHub:Token"]).Returns("fake-token");

            var loggerMock = new Mock<ILogger<GitHubService>>();
            var service = new GitHubService(client, configMock.Object, loggerMock.Object);

            // Act
            var result = await service.CloseIssueAsync("user/repo", 1);

            // Assert
            Assert.True(result);
        }
    }
}