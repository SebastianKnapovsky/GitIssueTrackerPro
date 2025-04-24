using GitIssueTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueTracker.Core.Services.Interfaces
{
    public interface IGitHubService
    {
        Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue);
        Task<IssueResponse> UpdateIssueAsync(string repository, int issueNumber, IssueRequest issue);
        Task<bool> CloseIssueAsync(string repository, int issueNumber);
    }

}
