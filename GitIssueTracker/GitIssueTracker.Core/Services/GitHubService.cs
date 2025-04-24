using GitIssueTracker.Core.Enums;
using GitIssueTracker.Core.Models;
using GitIssueTracker.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitIssueTracker.Core.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.github.com";
        private readonly string _token;
        private readonly ILogger<GitHubService> _logger;

        public GitHubService(HttpClient httpClient, IConfiguration config, ILogger<GitHubService> logger)
        {
            _httpClient = httpClient;
            _token = config["GitServices:GitHub:Token"] ?? throw new ArgumentException("Brak tokena GitHub w konfiguracji.");
            _logger = logger;

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitIssueTracker", "1.0"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
        public async Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue)
        {
            try
            {
                repository = Uri.UnescapeDataString(repository);

                var url = $"{_baseUrl}/repos/{repository}/issues";

                var payload = new
                {
                    title = issue.Title,
                    body = issue.Description
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                _logger.LogInformation("Pomyślnie utworzono issue dla repo {Repository}", repository);

                return new IssueResponse
                {
                    IssueNumber = json.GetProperty("number").GetInt32(),
                    Url = json.GetProperty("html_url").GetString(),
                    Status = IssueStatus.Open
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Błąd zapytania do GitHub API podczas tworzenia issue dla repo {Repository}", repository);
                throw new ApplicationException("GitHub API error");
            }
        }
        public async Task<IssueResponse> UpdateIssueAsync(string repository, int issueNumber, IssueRequest issue)
        {
            try
            {
                repository = Uri.UnescapeDataString(repository);

                var url = $"{_baseUrl}/repos/{repository}/issues/{issueNumber}";

                var payload = new
                {
                    title = issue.Title,
                    body = issue.Description
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync(url, content);
                response.EnsureSuccessStatusCode();

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                _logger.LogInformation("Zaktualizowano issue #{IssueNumber} w repo {Repository}", issueNumber, repository);

                var state = json.GetProperty("state").GetString()?.ToLower();
                return new IssueResponse
                {
                    IssueNumber = json.GetProperty("number").GetInt32(),
                    Url = json.GetProperty("html_url").GetString(),
                    Status = state == "closed" ? IssueStatus.Closed : IssueStatus.Open
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji issue #{IssueNumber} w repo {Repository}", issueNumber, repository);
                throw new ApplicationException("Błąd podczas aktualizacji zgłoszenia");
            }
        }
        public async Task<bool> CloseIssueAsync(string repository, int issueNumber)
        {
            try
            {
                repository = Uri.UnescapeDataString(repository);

                var url = $"{_baseUrl}/repos/{repository}/issues/{issueNumber}";

                var payload = new { state = "closed" };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(url, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Zamknięto issue #{IssueNumber} w repo {Repository}", issueNumber, repository);

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Błąd podczas zamykania issue #{IssueNumber} w repo {Repository}", issueNumber, repository);
                return false;
            }
        }
    }
}
