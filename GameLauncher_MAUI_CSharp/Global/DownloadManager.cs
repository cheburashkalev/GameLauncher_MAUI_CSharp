using Downloader;
using LiteDB;
using Octokit;

public static class DownloadManagerS
{
    public class DownloadGameStartEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }
    }
    public class DownloadGameProgressEventArgs : EventArgs
    {
        public DownloadProgressChangedEventArgs DownloadProgressArgs { get; set; }
        public ObjectId GameId { get; set; }
    }
    public class DownloadGameCompliteEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }
    }
    public class DownloadKey
    {
        public ObjectId? Gameid { get; set; }
        public int PartDownload { get; set; }
    }
    public static event EventHandler<DownloadGameProgressEventArgs> DownloadProgressChanged;
    public static event EventHandler<DownloadGameStartEventArgs> DownloadGameStart;
    public static event EventHandler<DownloadGameCompliteEventArgs> DownloadGameComplite;
    public static Dictionary<DownloadKey, IDownload> DownloadList = new();
    // public static Dictionary<ObjectId,>
    public static async Task DownloadGame(ObjectId gameID, Release release, ObjectId selectedDisk)
    {
        var rep = LauncherApp.db.GetCollection<Repositories>("Repositories").FindById(gameID);
        var releases = release.Assets.ToList();
        var GameCashDB_File = releases.Find(x => x.Name.StartsWith("GameCashDB") && x.Name.EndsWith(".db"));
        var SaveDir = LauncherApp.db.GetCollection("FoldersLibraryX").FindById(selectedDisk);
        var CurrentDownloadsGame = DownloadList.ToList().FindAll(x => x.Key.Gameid == gameID);
        if (CurrentDownloadsGame.Count != 0)
        {
            return;
        }
        if (GameCashDB_File != null)
        {
            DownloadList.Add(new DownloadKey() { Gameid = gameID, PartDownload = 0 },
            await GitHubDownloader.Download(
                GameCashDB_File.BrowserDownloadUrl,
                $"GameCashDB_{gameID.ToString()}.db",
                SaveDir["LibraryFolder"].AsString,
                (o, x) => { DownloadProgressChanged.Invoke(new object(), new DownloadGameProgressEventArgs() { GameId = gameID, DownloadProgressArgs = x }); },
                (o, x) =>
                {
                    DownloadList.Remove(new DownloadKey() { Gameid = gameID, PartDownload = 0 });
                    DownloadGameComplite.Invoke(new object(), new DownloadGameCompliteEventArgs() { GameId = gameID });

                },
                (o, x) => { DownloadGameStart.Invoke(new object(), new DownloadGameStartEventArgs() { GameId = gameID }); }
                ));
            return;
        }
        var PartsGameCashDB_File = releases.FindAll(x => x.Name.StartsWith("part_") && x.Name.EndsWith(".dat"));
        if (PartsGameCashDB_File != null)
        {
            try
            {
                await PartDownload(gameID, PartsGameCashDB_File, null, SaveDir["LibraryFolder"].AsString);
            }
            catch (Exception e) 
            {
            }
        }
    }
    private static async Task PartDownload(ObjectId GameId, List<ReleaseAsset>? AllParts, ReleaseAsset? lastDownload,string SaveDir)
    {
        if (AllParts != null)
        {
            ReleaseAsset? CurrentDownload;
            int partDownload = 0;
            if (lastDownload != null)
            {
                if (lastDownload != AllParts.Last())
                {
                    AllParts.IndexOf(lastDownload);
                    partDownload = AllParts.IndexOf(lastDownload) + 1;
                    CurrentDownload = AllParts[partDownload];
                }
                else
                {
                    return;
                    //провести тут установку игры
                }
            }
            else
            {
                CurrentDownload = AllParts.FirstOrDefault();
            }
            DownloadList.Add(new DownloadKey() { Gameid = GameId, PartDownload = partDownload },
await GitHubDownloader.Download(
    CurrentDownload.BrowserDownloadUrl,
    $"GameCashDB_{GameId.ToString()}_part{partDownload}.dat",
    SaveDir,
    (o, x) => { if (DownloadProgressChanged != null) DownloadProgressChanged.Invoke(new object(), new DownloadGameProgressEventArgs() { GameId = GameId, DownloadProgressArgs = x }); },
            async (o, x) =>
            {
               var OldDownload = DownloadList.Keys.FirstOrDefault(x => x.Gameid == GameId && x.PartDownload == partDownload);
                if (OldDownload != null)
                {
                    DownloadList.Remove(OldDownload);
                }
                await PartDownload(GameId, AllParts, CurrentDownload,SaveDir);
            },
    (o, x) => { if (DownloadGameStart != null) { DownloadGameStart.Invoke(new object(), new DownloadGameStartEventArgs() { GameId = GameId }); } }
    ));
        }
    }
}

