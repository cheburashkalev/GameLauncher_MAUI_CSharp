using GameLauncher_MAUI_CSharp.Global;
using Octokit;




namespace GameLauncher_MAUI_CSharp.Code.TorrentLib
{
    public struct Project
    {
        List<FilesInReliase> filesInReliases;
    }
    public struct FilesInReliase
    {
        public string FullGameZIP;
        public ulong SizeFullGameZIP;
        public string PatchGame;
        public ulong SizePatchGame;
        public string TorrentGame;
        public ulong SizeTorrentGame;
        public string URL_PNG_3_2;

    }
    public class TokenReciveEventArgs : EventArgs
    {
    }
    public static class TorrentDownloader
    {
        public static EventHandler<TokenReciveEventArgs>? TokenRecive;
        public static readonly GitHubClient client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
        public static bool DeliteTokenGitHub()
        {
            TorrentDownloader.client.Credentials = Credentials.Anonymous;
            var cl = LauncherApp.db.GetCollection<DB_OAuth>("OAuth");
            if (cl.Exists(x => x.AuthServise == "GitHub"))
            {
                var _DB_OAuth = cl.FindOne(x => x.AuthServise == "GitHub");
                _DB_OAuth.token = "";
                TokenRecive?.Invoke(new object(), new());
                return cl.Update(_DB_OAuth);
            }
            return false;
        }
       

        public static bool NewCodeFromGitHub(string Code)
        {
            var tasktoken = client.Oauth.CreateAccessToken(new OauthTokenRequest(LauncherApp.GITHUB_CLIENT_ID, LauncherApp.GITHUB_CLIENT_SECRETS, Code));
            tasktoken.Wait();
            OauthToken oauthToken = tasktoken.Result;
            if (oauthToken.AccessToken != null)
            {
                var cl = LauncherApp.db.GetCollection<DB_OAuth>("OAuth");
                if (cl.Exists(x => x.AuthServise == "GitHub"))
                {
                    var _DB_OAuth = cl.FindOne(x => x.AuthServise == "GitHub");
                    // _repositories.RepId = ObjectId.NewObjectId();
                    _DB_OAuth.token = oauthToken.AccessToken;
                    cl.Update(_DB_OAuth);
                }
                else
                {
                    var _DB_OAuth = new DB_OAuth
                    {
                        AuthServise = "GitHub",
                        token = oauthToken.AccessToken,
                    };
                    cl.Insert(_DB_OAuth);
                    var _DB_OAuthEmpty = new DB_OAuth
                    {
                        AuthServise = "",
                        token = "",
                    };
                    cl.Insert(_DB_OAuthEmpty);
                    cl.EnsureIndex(x => x.AuthServise);
                }
                client.Credentials = new Credentials(oauthToken.AccessToken);
                TokenRecive?.Invoke(new object(), new());
                return true;

            }
            return false;
        }
        public static async Task DownloadAsync(CancellationToken token)
        {
            ProjectsJsonURL testProjectsJsonUrl = new ProjectsJsonURL(1);
            var downloadsPath = Path.Combine(Environment.CurrentDirectory, "Downloads");

            // .torrent files will be loaded from this directory (if any exist)
            var torrentsPath = Path.Combine(Environment.CurrentDirectory, "Torrents");
            if (!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);


        }
        public static bool UserValid(string user)
        {
            try
            {

                var result = client.User.Get(user);
                result.Wait();
                return true;
            }
            catch
            {
                return false;
            }

        }
        public static bool RepValid(string user, string rep)
        {
            try
            {
                var result = client.Repository.Get(user, rep);
                result.Wait();
                return true;
            }
            catch
            {
                return false;
            }

        }
        public static List<List<FilesInReliase>> GetInfoReliases()
        {
            List<List<FilesInReliase>> Projects = new();
            //foreach()
            var releases = client.Repository.Release.GetAll("cheburashkalev", "testRepForLaucherFlax");
            releases.Wait();

            List<FilesInReliase> filesInReliase_s = new();
            foreach (var release in releases.Result)
            {
                FilesInReliase filesInReliase = new();
                foreach (var asset in release.Assets)
                {
                    if (asset.Name.EndsWith(".zip"))
                    {
                        filesInReliase.SizeFullGameZIP = (ulong)asset.Size;
                        filesInReliase.FullGameZIP = asset.BrowserDownloadUrl;
                        continue;
                    }
                    if (asset.Name.EndsWith("3_2.png"))
                    {
                        filesInReliase.URL_PNG_3_2 = asset.BrowserDownloadUrl;
                        continue;
                    }
                    else if (asset.Name.EndsWith(".patch"))
                    {
                        filesInReliase.SizePatchGame = (ulong)asset.Size;
                        filesInReliase.PatchGame = asset.BrowserDownloadUrl;
                        continue;
                    }
                    else if (asset.Name.EndsWith(".torrent"))
                    {
                        filesInReliase.SizeTorrentGame = (ulong)asset.Size;
                        filesInReliase.TorrentGame = asset.BrowserDownloadUrl;
                        continue;
                    }
                }
                filesInReliase_s.Add(filesInReliase);
            }
            Projects.Add(filesInReliase_s);

            return Projects;
        }
    }
}
