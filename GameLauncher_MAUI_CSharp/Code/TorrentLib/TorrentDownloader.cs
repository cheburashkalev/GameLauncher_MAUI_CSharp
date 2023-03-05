using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Logging;
using GameLauncher_MAUI_CSharp.Global;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;


namespace GameLauncher_MAUI_CSharp.Code.TorrentLib
{
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
    public static class TorrentDownloader
    {
       static GitHubClient client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
        public static async Task DownloadAsync(CancellationToken token)
        {
            ProjectsJsonURL testProjectsJsonUrl = new ProjectsJsonURL(1);
            var downloadsPath = Path.Combine(Environment.CurrentDirectory, "Downloads");

            // .torrent files will be loaded from this directory (if any exist)
            var torrentsPath = Path.Combine(Environment.CurrentDirectory, "Torrents");
            if (!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);


        }
        public static List<FilesInReliase> GetInfoReliases() 
        {
            var releases = client.Repository.Release.GetAll("cheburashkalev", "testRepForLaucherFlax");
            releases.Wait();
            List<FilesInReliase> filesInReliase_s = new();
            foreach (var release in releases.Result)
            {
                FilesInReliase filesInReliase =new();
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
            

            return filesInReliase_s;
        }
    }
}
