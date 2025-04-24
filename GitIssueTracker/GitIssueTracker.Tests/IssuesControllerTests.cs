using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitIssueTracker.Api.Controllers;
using GitIssueTracker.Core.Enums;
using GitIssueTracker.Core.Models;
using GitIssueTracker.Core.Services;
using GitIssueTracker.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace GitIssueTracker.Tests
{
    public class IssuesControllerTests
    {
        #region GitHub

        [Fact]
        public async Task CreateIssueGitHub_ShouldReturnOk_WithValidResponse()
        {
            // Arrange
            var gitHubMock = new Mock<IGitHubService>();
            var gitLabMock = new Mock<IGitLabService>(); // nieużywane, ale potrzebne
            var loggerMock = new Mock<ILogger<IssuesController>>();

            var request = new IssueRequest
            {
                Title = "Test issue",
                Description = "Test desc"
            };

            gitHubMock.Setup(s => s.CreateIssueAsync("repo", request))
                      .ReturnsAsync(new IssueResponse
                      {
                          IssueNumber = 1,
                          Url = "https://github.com/repo/issues/1",
                          Status = IssueStatus.Open
                      });

            var controller = new IssuesController(gitHubMock.Object, gitLabMock.Object, loggerMock.Object);

            // Act
            var result = await controller.CreateIssueGitHub("repo", request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<IssueResponse>(ok.Value);
            Assert.Equal(1, response.IssueNumber);
        }

        [Fact]
        public async Task UpdateIssueGitHub_ShouldReturnOk()
        {
            // ARRANGE
            var gitHubMock = new Mock<IGitHubService>();
            var gitLabMock = new Mock<IGitLabService>();
            var loggerMock = new Mock<ILogger<IssuesController>>();

            var updated = new IssueResponse
            {
                IssueNumber = 1,
                Url = "https://github.com/repo/issues/1",
                Status = IssueStatus.Open
            };

            gitHubMock.Setup(s => s.UpdateIssueAsync("repo", 1, It.IsAny<IssueRequest>()))
                      .ReturnsAsync(updated);

            var controller = new IssuesController(gitHubMock.Object, gitLabMock.Object, loggerMock.Object);

            // ACT
            var result = await controller.UpdateIssueGitHub("repo", 1, new IssueRequest
            {
                Title = "Updated title",
                Description = "Updated desc"
            });

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task CloseIssueGitHub_ShouldReturnNoContent()
        {
            // ARRANGE
            var gitHubMock = new Mock<IGitHubService>();
            var gitLabMock = new Mock<IGitLabService>();
            var loggerMock = new Mock<ILogger<IssuesController>>();

            gitHubMock.Setup(s => s.CloseIssueAsync("repo", 1))
                      .ReturnsAsync(true);

            var controller = new IssuesController(gitHubMock.Object, gitLabMock.Object, loggerMock.Object);

            // ACT
            var result = await controller.CloseIssueGitHub("repo", 1);

            // ASSERT
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region GitLab

        [Fact]
        public async Task CreateIssueGitLab_ShouldReturnOk_WithValidResponse()
        {
            // ARRANGE
            var gitHubMock = new Mock<IGitHubService>();
            var gitLabMock = new Mock<IGitLabService>();
            var loggerMock = new Mock<ILogger<IssuesController>>();

            var request = new IssueRequest
            {
                Title = "GitLab test",
                Description = "Some desc"
            };

            var expected = new IssueResponse
            {
                IssueNumber = 42,
                Url = "https://gitlab.com/issues/42",
                Status = IssueStatus.Open
            };

            gitLabMock.Setup(s => s.CreateIssueAsync("repo", request))
                      .ReturnsAsync(expected);

            var controller = new IssuesController(gitHubMock.Object, gitLabMock.Object, loggerMock.Object);

            // ACT
            var result = await controller.CreateIssueGitLab("repo", request);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<IssueResponse>(ok.Value);
            Assert.Equal(42, response.IssueNumber);
        }

        [Fact]
        public async Task UpdateIssueGitLab_ShouldReturnOk()
        {
            // ARRANGE
            var gitHubMock = new Mock<IGitHubService>();
            var gitLabMock = new Mock<IGitLabService>();
            var loggerMock = new Mock<ILogger<IssuesController>>();

            var updated = new IssueResponse
            {
                IssueNumber = 101,
                Url = "https://gitlab.com/issues/101",
                Status = IssueStatus.Open
            };

            gitLabMock.Setup(s => s.UpdateIssueAsync("repo", 101, It.IsAny<IssueRequest>()))
                      .ReturnsAsync(updated);

            var controller = new IssuesController(gitHubMock.Object, gitLabMock.Object, loggerMock.Object);

            // ACT
            var result = await controller.UpdateIssueGitLab("repo", 101, new IssueRequest
            {
                Title = "Updated",
                Description = "Updated desc"
            });

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task CloseIssueGitLab_ShouldReturnNoContent()
        {
            // ARRANGE
            var gitHubMock = new Mock<IGitHubService>();
            var gitLabMock = new Mock<IGitLabService>();
            var loggerMock = new Mock<ILogger<IssuesController>>();

            gitLabMock.Setup(s => s.CloseIssueAsync("repo", 5)).ReturnsAsync(true);

            var controller = new IssuesController(gitHubMock.Object, gitLabMock.Object, loggerMock.Object);

            // ACT
            var result = await controller.CloseIssueGitLab("repo", 5);

            // ASSERT
            Assert.IsType<NoContentResult>(result);
        }

        #endregion
    }
}
