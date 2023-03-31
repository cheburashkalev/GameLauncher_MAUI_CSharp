using GameLauncher_MAUI_CSharp.Code.TorrentLib;
using LiteDB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebSocketSharp;
using WebSocketSharp.Server;

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

public class WebLauncherRecieve : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        // access_token=123&expires_in=0&user_id=456


        TorrentDownloader.NewCodeFromGitHub(e.Data.Split("code=")[1]);
        LauncherApp.test = e.Data;

    }
}
public class DB_OAuth
{
    [BsonId]
    public string AuthServise { get;set; }
    public string token { get; set; }
}
