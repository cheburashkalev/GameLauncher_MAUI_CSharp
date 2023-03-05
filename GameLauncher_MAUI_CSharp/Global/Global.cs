using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher_MAUI_CSharp.Global
{
    public struct ProjectsJsonURL
    {
        public string GitHubUserUrl;
        public string GitHubRepUrl;
        public string GitHubBranch;
        public string GitUrl { get; }


        public ProjectsJsonURL(string GitHubUserUrl, string GitHubRepUrl, string GitHubBranch)
        {
            this.GitHubUserUrl = GitHubUserUrl;
            this.GitHubRepUrl = GitHubRepUrl;
            this.GitHubBranch = GitHubBranch;
            this.GitUrl = "https://api.github.com/repos/";
        }
        public ProjectsJsonURL(int a)
        {
            //https://api.github.com/repos/OWNER/REPO/releases
            GitHubUserUrl = "cheburashkalev";
            GitHubRepUrl = "testRepForLaucherFlax";
            GitHubBranch = "releases";
            this.GitUrl = "https://api.github.com/repos/";
        }

        public string getUrl(string PathFile)
        {
            return GitUrl + GitHubUserUrl + "/" + GitHubRepUrl + "/" + GitHubBranch;
        }
    }
    public static class Global
    {
     
        public static int MaxNumberOfItems = 100;
        public static string AppName = "My App";
    }
}
