using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCController.Core.Models
{
    public class GitHubRepoModel
    {
        public string RepoName { get; set; }
        public string RepoGitUrl { get; set; }
        public string Description { get; set; }
        public string HtmlUrl { get; set; }
        public string Homepage { get; set; }
        public bool HasPages { get; set; }
        public bool HasWiki { get; set; }
    }
}
