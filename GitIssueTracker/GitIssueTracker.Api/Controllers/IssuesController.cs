using GitIssueTracker.Core.Enums;
using GitIssueTracker.Core.Models;
using GitIssueTracker.Core.Services;
using GitIssueTracker.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GitIssueTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssuesController : Controller
    {
        private readonly IGitHubService _gitHubService;
        private readonly IGitLabService _gitLabService;
        private readonly ILogger<IssuesController> _logger;
        public IssuesController(IGitHubService gitHubService, IGitLabService gitLabService, ILogger<IssuesController> logger)
        {
            _gitHubService = gitHubService;
            _gitLabService = gitLabService;
            _logger = logger;
        }

        #region GitHub

        [HttpPost("github/{repository}")]
        public async Task<IActionResult> CreateIssueGitHub(string repository, [FromBody] IssueRequest request)
        {
            _logger.LogInformation("Tworzenie zgłoszenia GitHub w repozytorium {Repository}", repository);

            try
            {
                var response = await _gitHubService.CreateIssueAsync(repository, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia zgłoszenia w GitHub repozytorium {Repository}", repository);
                return StatusCode(500, "Wystąpił błąd podczas tworzenia zgłoszenia GitHub.");
            }
        }
        [HttpPut("github/{repository}/{issueNumber}")]
        public async Task<IActionResult> UpdateIssueGitHub(string repository, int issueNumber, [FromBody] IssueRequest request)
        {
            _logger.LogInformation("Aktualizacja zgłoszenia GitHub #{IssueNumber} w repo {Repository}", issueNumber, repository);

            try
            {
                var response = await _gitHubService.UpdateIssueAsync(repository, issueNumber, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji zgłoszenia GitHub #{IssueNumber} w repo {Repository}", issueNumber, repository);
                return StatusCode(500, "Wystąpił błąd podczas aktualizacji zgłoszenia GitHub.");
            }
        }
        [HttpDelete("github/{repository}/{issueNumber}")]
        public async Task<IActionResult> CloseIssueGitHub(string repository, int issueNumber)
        {
            _logger.LogInformation("Zamykanie zgłoszenia GitHub #{IssueNumber} w repo {Repository}", issueNumber, repository);

            try
            {
                var result = await _gitHubService.CloseIssueAsync(repository, issueNumber);
                if (result)
                {
                    return NoContent();
                }

                _logger.LogWarning("Zamknięcie zgłoszenia GitHub #{IssueNumber} nie powiodło się w repo {Repository}", issueNumber, repository);
                return StatusCode(500, "Nie udało się zamknąć zgłoszenia.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zamykania zgłoszenia GitHub #{IssueNumber} w repo {Repository}", issueNumber, repository);
                return StatusCode(500, "Wystąpił błąd podczas zamykania zgłoszenia.");
            }
        }

        #endregion

        #region GitLab
        [HttpPost("gitlab/{repository}")]
        public async Task<IActionResult> CreateIssueGitLab(string repository, [FromBody] IssueRequest request)
        {
            _logger.LogInformation("Tworzenie zgłoszenia GitLab w repozytorium: {Repository}", repository);
            try
            {
                var result = await _gitLabService.CreateIssueAsync(repository, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia zgłoszenia w GitLab repo: {Repository}", repository);
                return StatusCode(500, "Wystąpił błąd podczas tworzenia zgłoszenia.");
            }
        }

        [HttpPut("gitlab/{repository}/{issueNumber}")]
        public async Task<IActionResult> UpdateIssueGitLab(string repository, int issueNumber, [FromBody] IssueRequest request)
        {
            _logger.LogInformation("Aktualizacja zgłoszenia GitLab #{IssueNumber} w repo {Repository}", issueNumber, repository);
            try
            {
                var result = await _gitLabService.UpdateIssueAsync(repository, issueNumber, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji zgłoszenia GitLab #{IssueNumber} w repo {Repository}", issueNumber, repository);
                return StatusCode(500, "Wystąpił błąd podczas aktualizacji zgłoszenia.");
            }
        }

        [HttpDelete("gitlab/{repository}/{issueNumber}")]
        public async Task<IActionResult> CloseIssueGitLab(string repository, int issueNumber)
        {
            _logger.LogInformation("Zamykanie zgłoszenia GitLab #{IssueNumber} w repo {Repository}", issueNumber, repository);
            try
            {
                var result = await _gitLabService.CloseIssueAsync(repository, issueNumber);
                if (result)
                    return NoContent();

                _logger.LogWarning("Zamknięcie zgłoszenia GitLab #{IssueNumber} nie powiodło się w repo: {Repository}", issueNumber, repository);
                return StatusCode(500, "Nie udało się zamknąć zgłoszenia.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zamykania zgłoszenia GitLab #{IssueNumber} w repo: {Repository}", issueNumber, repository);
                return StatusCode(500, "Wystąpił błąd podczas zamykania zgłoszenia.");
            }
        }
        #endregion
    }
}
