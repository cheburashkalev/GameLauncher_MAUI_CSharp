using Downloader;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DownloadManager
{
	public static Dictionary<ObjectId, IDownload> DownloadList;
	public static async Task DownloadGame(ObjectId gameID) 
	{
		var rep = LauncherApp.db.GetCollection<Repositories>("Repositories").FindById(gameID);
		//await GitHubDownloader.Download();
	}
}

