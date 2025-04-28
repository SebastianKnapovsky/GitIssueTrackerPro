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
    public class GitLabService : IGitLabService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly ILogger<GitLabService> _logger;

        public GitLabService(HttpClient httpClient, IConfiguration config, ILogger<GitLabService> logger)
        {
            _httpClient = httpClient;
            _token = config["GitServices:GitLab:Token"] ?? throw new ArgumentException("Brak tokena GitLab w konfiguracji.");
            _logger = logger;

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitIssueTracker", "1.0"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _httpClient.BaseAddress = new Uri("https://gitlab.com/api/v4/");
        }
        public async Task<List<IssueResponse>> GetIssuesAsync(string repository)
        {
            try
            {
                var url = $"projects/{repository}/issues";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                var issues = new List<IssueResponse>();

                foreach (var issue in json.EnumerateArray())
                {
                    issues.Add(new IssueResponse
                    {
                        IssueNumber = issue.GetProperty("iid").GetInt32(),
                        Url = issue.GetProperty("web_url").GetString() ?? "",
                        Status = issue.GetProperty("state").GetString()?.ToLower() == "closed" ? IssueStatus.Closed : IssueStatus.Open
                    });
                }

                _logger.LogInformation("Pobrano {Count} zgłoszeń z GitLab dla repozytorium {Repository}", issues.Count, repository);

                return issues;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania zgłoszeń z GitLab dla repo {Repository}", repository);
                throw new ApplicationException("GitLab API error podczas pobierania zgłoszeń");
            }
        }
        public async Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue)
        {
            try
            {
                var url = $"projects/{repository}/issues";

                var payload = new
                {
                    title = issue.Title,
                    description = issue.Description
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                return new IssueResponse
                {
                    IssueNumber = json.GetProperty("iid").GetInt32(),
                    Url = json.GetProperty("web_url").GetString() ?? "",
                    Status = IssueStatus.Open
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia zgłoszenia w GitLab.");
                throw;
            }
        }

        public async Task<IssueResponse> UpdateIssueAsync(string repository, int issueNumber, IssueRequest issue)
        {
            try
            {
                var url = $"projects/{repository}/issues/{issueNumber}";

                var payload = new
                {
                    title = issue.Title,
                    description = issue.Description
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                return new IssueResponse
                {
                    IssueNumber = json.GetProperty("iid").GetInt32(),
                    Url = json.GetProperty("web_url").GetString() ?? "",
                    Status = IssueStatus.Open
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji zgłoszenia w GitLab.");
                throw;
            }
        }

        public async Task<bool> CloseIssueAsync(string repository, int issueNumber)
        {
            try
            {
                var url = $"projects/{repository}/issues/{issueNumber}";

                var payload = new
                {
                    state_event = "close"
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zamykania zgłoszenia w GitLab.");
                return false;
            }
        }
    }
}
