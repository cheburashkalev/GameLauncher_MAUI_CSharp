using Downloader;
using LiteDB;
using Octokit;
using System.Text.RegularExpressions;


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
        public int Part { get; set; }
        public IReadOnlyList<ReleaseAsset> GameAssets { get; set; }
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


    //CombineFiles


    public class CombineFileStartEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }
    }
    public class CombineFileProgressEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }
        public int ProgressPercent { get; set; }
        public int Part { get; set; }
    }
    public class CombineFileEndEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }

    }
    public enum UnpackState
    {
        None = 0,
        Created = 1,
        Running = 2,
        Completed = 3,
        Failed = 4
    }
    //InstallProcess
    public class InstallStartEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }
    }
    public class InstallProgressEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }
        public int ProgressPercent { get; set; }
        public int Part { get; set; }
    }
    public class InstallEndEventArgs : EventArgs
    {
        public ObjectId GameId { get; set; }

    }
    public class DownloadValue
    {
        public IDownload downloadIntf { get; set; }
        public UnpackState UnpackState { get; set; }
        public UnpackState InstallState { get; set; }
    }
    public static event EventHandler<DownloadGameProgressEventArgs> DownloadProgressChanged;
    public static event EventHandler<DownloadGameStartEventArgs> DownloadGameStart;
    public static event EventHandler<DownloadGameCompliteEventArgs> DownloadGameComplite;
    public static event EventHandler<CombineFileStartEventArgs> CombineFileStart;
    public static event EventHandler<CombineFileProgressEventArgs> CombineFileProgress;
    public static event EventHandler<CombineFileEndEventArgs> CombineFileComplite;
    public static event EventHandler<InstallStartEventArgs> InstallStart;
    public static event EventHandler<InstallProgressEventArgs> InstallProgress;
    public static event EventHandler<InstallEndEventArgs> InstallComplite;
    public static Dictionary<DownloadKey, DownloadValue> DownloadList = new();
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
            DownloadList.Add(new DownloadKey() { Gameid = gameID, PartDownload = 0 }, new DownloadValue()
            {
                downloadIntf =
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
                ),
                UnpackState = UnpackState.Created,
            });
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
    private static async Task PartDownload(ObjectId GameId, List<ReleaseAsset>? AllParts, ReleaseAsset? lastDownload, string SaveDir)
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
                    bool stopTimer = false;
                    PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                    while (!stopTimer)
                    {
                        await timer.WaitForNextTickAsync();
                        var gamedownloadlist = DownloadList.Where(x => x.Key.Gameid == GameId);
                        if (gamedownloadlist != null && gamedownloadlist.Count() == gamedownloadlist.Where(x => x.Value.UnpackState == UnpackState.Completed).Count())
                        {
                            await InstallFile($"{SaveDir}GameCashDB_{GameId.ToString()}.db", $"{SaveDir + GameId.ToString()}\\", GameId);
                            stopTimer = true;
                        }

                    }
                    stopTimer = false;
                    while (!stopTimer)
                    {
                        await timer.WaitForNextTickAsync();
                        var gamedownloadlist = DownloadList.Where(x => x.Key.Gameid == GameId);
                        if (gamedownloadlist != null && gamedownloadlist.Count() == gamedownloadlist.Where(x => x.Value.UnpackState == UnpackState.Completed && x.Value.InstallState == UnpackState.Completed).Count())
                        {

                            foreach (var gamedownload in gamedownloadlist)
                            {
                                DownloadList.Remove(gamedownload.Key);
                            }
                            if (DownloadGameComplite != null)
                                DownloadGameComplite.Invoke(new object(), new DownloadGameCompliteEventArgs() { GameId = GameId });
                            stopTimer = true;
                        }

                    }
                    return;
                    //провести тут установку игры
                }
            }
            else
            {
                CurrentDownload = AllParts.FirstOrDefault();
            }
            DownloadList.Add(new DownloadKey() { Gameid = GameId, PartDownload = partDownload },
new DownloadValue()
{
    downloadIntf = await GitHubDownloader.Download(
    CurrentDownload.BrowserDownloadUrl,
    $"GameCashDB_{GameId.ToString()}_part{partDownload}.dat",
    SaveDir,
    (o, x) => { if (DownloadProgressChanged != null) DownloadProgressChanged.Invoke(new object(), new DownloadGameProgressEventArgs() { GameId = GameId, DownloadProgressArgs = x, Part = partDownload, GameAssets = AllParts }); },
            async (o, x) =>
            {

                PartDownload(GameId, AllParts, CurrentDownload, SaveDir);
                if (partDownload > 0)
                {
                    bool stopTimer = false;
                    PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

                    while (!stopTimer)
                    {
                        await timer.WaitForNextTickAsync();
                        var OldDownload = DownloadList.FirstOrDefault(x => x.Key.Gameid == GameId && x.Key.PartDownload == partDownload - 1 && x.Value.UnpackState == UnpackState.Completed);

                        if (OldDownload.Key != null && OldDownload.Value.downloadIntf.Status == DownloadStatus.Completed)
                        {
                            OldDownload = DownloadList.FirstOrDefault(x => x.Key.Gameid == GameId && x.Key.PartDownload == partDownload);
                            await CombineFile(SaveDir + $"GameCashDB_{GameId.ToString()}_part{partDownload}.dat", SaveDir + $"GameCashDB_{GameId.ToString()}.db", OldDownload.Key);
                            stopTimer = true;

                        }
                    }
                    timer.Dispose();
                }
                else
                {
                    var OldDownload = DownloadList.FirstOrDefault(x => x.Key.Gameid == GameId && x.Key.PartDownload == partDownload);

                    await CombineFile(SaveDir + $"GameCashDB_{GameId.ToString()}_part{partDownload}.dat", SaveDir + $"GameCashDB_{GameId.ToString()}.db", OldDownload.Key);

                }

            },
    (o, x) => { if (DownloadGameStart != null) { DownloadGameStart.Invoke(new object(), new DownloadGameStartEventArgs() { GameId = GameId }); } }
    ),
    UnpackState = UnpackState.None,
});
        }
    }
    public static IReadOnlyList<ReleaseAsset> GetSortedPartsGame(IReadOnlyList<ReleaseAsset> releaseAssets)
    {
        var PartsAssets = releaseAssets.Where(x => x.Name.StartsWith("part_") && x.Name.EndsWith(".dat"));
        string pattern = @"\d+";

        // Create a Regex object with the pattern
        Regex regex = new Regex(pattern);
        PartsAssets.ToList().Sort((x, y) =>
        {
            // Try to find a match in the first string
            Match mx = regex.Match(x.Name);

            // Try to find a match in the second string
            Match my = regex.Match(y.Name);
            if (mx.Success && my.Success)
            {
                int nx = int.Parse(mx.Value);
                int ny = int.Parse(my.Value);
                return nx.CompareTo(ny);
            }
            else
            {
                // If one or both strings do not have a match, use the default string comparison
                return x.Name.CompareTo(y.Name);
            }
        });
        return PartsAssets.ToList();

    }
    public static long GameSize(IReadOnlyList<ReleaseAsset> GameAssets)
    {
        long Size = 0;
        var PartsGame = GameAssets.Where(x => x.Name.EndsWith(".dat") && x.Name.StartsWith("part"));
        if (PartsGame.Count() != 0)
        {
            foreach (var part in PartsGame)
            {
                Size += part.Size;
            }
            return Size;
        }
        var GameCash = GameAssets.First(x => x.Name.EndsWith(".db") && x.Name.StartsWith("GameCashDB"));
        if (GameCash != null)
        {
            return GameCash.Size;
        }
        return 0;
    }
    public static async Task CombineFile(string PartFile, string destinationFile, DownloadKey downloadKey)
    {
        var DownloadPart = DownloadList.FirstOrDefault(x => x.Key == downloadKey);

        using (FileStream destinationStream = new FileStream(destinationFile, System.IO.FileMode.Append))
        {
            FileInfo fileInfo = new FileInfo(PartFile);
            if (fileInfo.Exists && fileInfo.Extension == ".dat")
            {
                DownloadPart.Value.UnpackState = UnpackState.Created;

                if (CombineFileStart != null)
                    CombineFileStart.Invoke(new object(), new CombineFileStartEventArgs() { GameId = downloadKey.Gameid });
                using (FileStream sourceStream = new FileStream(PartFile, System.IO.FileMode.Open))
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int bytesRead;
                    DownloadPart.Value.UnpackState = UnpackState.Running;

                    long current_size = 0;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        current_size += bytesRead;
                        if (CombineFileProgress != null)
                            CombineFileProgress.Invoke(new object(), new CombineFileProgressEventArgs() { GameId = downloadKey.Gameid, ProgressPercent = Convert.ToInt32(current_size / (fileInfo.Length / 100)), Part = DownloadPart.Key.PartDownload });
                        await destinationStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
                DownloadPart.Value.UnpackState = UnpackState.Completed;
                if (CombineFileComplite != null)
                    CombineFileComplite.Invoke(new object(), new CombineFileEndEventArgs() { GameId = downloadKey.Gameid });
                fileInfo.Delete();
            }
        }
    }
    private static async Task InstallFile(string GameCashDB_Path, string PathInstall, ObjectId GameId)
    {
        LiteDatabase GameDB = new LiteDatabase(GameCashDB_Path);
        ILiteStorage<string> storage = GameDB.GetStorage<string>("GameFiles", "GameFileChunks");
        var AllFiles = storage.FindAll();
        bool stopWhile = false;
        while (!stopWhile)
        {
            var file = AllFiles.FirstOrDefault();
            if (file != null)
            {
                try
                {
                    System.IO.FileInfo Prefile = new System.IO.FileInfo(PathInstall + file.Metadata["relativePath"].AsString);
                    Prefile.Directory.Create();
                    file.SaveAs(PathInstall + file.Metadata["relativePath"].AsString);
                    storage.Delete(file.Id);
                }
                catch(Exception x) 
                {

                }

            }
            else
            {
                var GamePack = DownloadList.Where(x => x.Key.Gameid == GameId);
                foreach (var part in GamePack)
                {
                    part.Value.InstallState = UnpackState.Completed;
                }
                GameDB.Dispose();
                File.Delete(GameCashDB_Path);
                stopWhile = true;
            }
        }
    }
}

