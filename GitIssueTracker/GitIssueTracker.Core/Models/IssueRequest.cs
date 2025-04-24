using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueTracker.Core.Models
{
    public class IssueRequest
    {
        public required string Title {  get; set; }
        public required string Description { get; set; }
    }
}
