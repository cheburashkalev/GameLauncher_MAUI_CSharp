using GameLauncher_MAUI_CSharp.Data;
using LiteDB;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using WebSocketSharp;
using WebSocketSharp.Server;
using Windows.Storage;

public static class LauncherApp
{
    public static string test;
    public static string GITHUB_CLIENT_ID = "945ac4987b3311821363";
    public static string GITHUB_CLIENT_SECRETS = "c076183289fa6989aec355335e60bfc5bdb4737f";
    public static string redirect_uri = "x-open-laucher://oauth";
    public static string AppPath = AppContext.BaseDirectory + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe";

    public static readonly LiteDatabase db = new LiteDatabase(GetDataBasePath());

    public static string MakeLibraryPath(string path)
    {
        return Path.Combine(path, "OpenXLiblary\\");
    }
    public static string GetAppDataDir()
    {
        string appData = ApplicationData.Current.LocalCacheFolder.Path;
        if (!Directory.Exists(Path.Combine(appData, "XOpenLauncher\\")))
        {
            Directory.CreateDirectory(Path.Combine(appData, "XOpenLauncher\\"));
        }
        return Path.Combine(appData, "XOpenLauncher\\");
    }
    public static string GetDataBasePath() => Path.Combine(GetAppDataDir(), "app.db");

    public static IEnumerable<DiskInfo> GetAllDisks()
    {
        List<DiskInfo> result = new List<DiskInfo>();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            if (drive.IsReady && drive.DriveType == DriveType.Fixed)
            {
                result.Add(new DiskInfo()
                {
                    Name = drive.VolumeLabel,
                    RootDirectory = drive.RootDirectory.FullName,
                    SizeBytes = drive.TotalSize,
                    AvailableSpaceBytes = drive.AvailableFreeSpace
                });
            }
        }
        return result;
    }

    public static int GetAvailablePort(int startingPort)
    {
        IPEndPoint[] endPoints;
        List<int> portArray = new List<int>();

        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

        //getting active connections
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        portArray.AddRange(from n in connections
                           where n.LocalEndPoint.Port >= startingPort
                           select n.LocalEndPoint.Port);

        //getting active tcp listners - WCF service listening in tcp
        endPoints = properties.GetActiveTcpListeners();
        portArray.AddRange(from n in endPoints
                           where n.Port >= startingPort
                           select n.Port);

        //getting active udp listeners
        endPoints = properties.GetActiveUdpListeners();
        portArray.AddRange(from n in endPoints
                           where n.Port >= startingPort
                           select n.Port);

        portArray.Sort();

        for (int i = startingPort; i < UInt16.MaxValue; i++)
            if (!portArray.Contains(i))
                return i;

        return 0;
    }

    public static void TryConnecntAndSendNewArg(string arg, bool checkOpenedApp)
    {
        RegistryKey rootKey = Registry.CurrentUser;
        RegistryKey uriKey = rootKey.CreateSubKey($@"Software\OpenLauncher", false);
        var port = (int)uriKey.GetValue("Port", 444);
        using (var ws = new WebSocket($"ws://{IPAddress.Loopback}:{port}/WebLauncherRecieve"))
        {

            ws.OnOpen += (sender, e) =>
            {
                if (!checkOpenedApp)
                    ws.Send(arg);
                Environment.Exit(0);
            };
            ws.Connect();
            ws.Close();

        }
    }

    public static void StartServer()
    {
        RegistryKey rootKey = Registry.CurrentUser;
        RegistryKey uriKey = rootKey.CreateSubKey($@"Software\OpenLauncher", true);
        int port = GetAvailablePort(444);
        uriKey.SetValue("Port", port);
        uriKey.Close();
        var wssv = new WebSocketServer(IPAddress.Loopback, port);
        wssv.AddWebSocketService<WebLauncherRecieve>("/WebLauncherRecieve");
        wssv.Start();
    }

    public static void AddGameId(string RootDirectory, ObjectId GameId)
    {
        BsonValue gameIdBson = new BsonValue(GameId);
        ILiteCollection<BsonDocument> cl = LauncherApp.db.GetCollection("FoldersLibraryX");
        BsonDocument? Library = cl.FindOne(x => x["RootDirectory"].AsString == RootDirectory);
        if (Library == null)
        {
            //TODO: Throw exception
            return;
        }

        BsonValue? libFolder = Library["LibraryFolder"];
        if (libFolder == null)
        {
            //TODO: Throw exception
            return;
        }
        BsonValue? rootDir = Library["RootDirectory"];
        if (rootDir == null)
        {
            //TODO: Throw exception
            return;
        }

        if (!Directory.Exists(libFolder.AsString))
        {
            //TODO: Throw exception
            return;
        }
        BsonValue? gameids = Library["GameIds"];
        BsonArray? gameidsArray = gameids != null ? gameids.AsArray : null;
        if (gameids == null || gameidsArray == null)
        {
            gameidsArray = new BsonArray();
            Library["GameIds"] = gameidsArray;
        }
        gameidsArray.Add(gameIdBson);
        cl.Update(Library["_id"], Library);
    }

    public static void SaveDirForDrive(string RootDirectory, string LibraryFolder)
    {
        BsonValue id = new BsonValue(ObjectId.NewObjectId());
        var cl = db.GetCollection("FoldersLibraryX");
        BsonDocument? Library = cl.FindOne(x => x["RootDirectory"].AsString == RootDirectory);
        if (!LibraryFolder.StartsWith(RootDirectory))
        {
            //TODO: Throw exception
            return;
        }
        Directory.CreateDirectory(LibraryFolder);
        if (Library == null)
        {
            cl.Insert(id,
             new BsonDocument
             {
                 ["RootDirectory"] = RootDirectory,
                 ["LibraryFolder"] = LibraryFolder,
                 ["GameIds"] = new BsonArray()
             });
            cl.EnsureIndex("_id");
        }
        else
        {
            if (Library["LibraryFolder"].AsString != LibraryFolder)
            {
                Library["LibraryFolder"] = LibraryFolder;
                id = Library["_id"];
                cl.Update(id, Library);
            }
        }
    }
    public static IEnumerable<BsonDocument>? GetGameLibrarys()
    {
        var cl = db.GetCollection("FoldersLibraryX");
        return cl.FindAll();
    }
    public static Dictionary<ObjectId, Process> RunningApps = new();
    public static async Task<bool> StartInstalledApp(ObjectId AppID, string arg)
    {
        try
        {
            string path = Path.Combine(LauncherApp.GetAppDataDir(), AppID.ToString() + ".db");
            LiteDatabase InfoDatabase = new LiteDatabase(path);
            var a = GetGameLibrarys();
            var aa = a.FirstOrDefault(x => x["GameIds"].AsArray.FirstOrDefault(x => x.AsObjectId.ToString() == AppID.ToString()).IsObjectId);
            using (var managementClass = new ManagementClass("Win32_Process"))
            {
                var processInfo = new ManagementClass("Win32_ProcessStartup");
                processInfo.Properties["CreateFlags"].Value = 0x00000008; // DETACHED_PROCESS flag

                var inParameters = managementClass.GetMethodParameters("Create");
                var b = Path.GetFullPath(aa["LibraryFolder"].AsString + AppID.ToString() + "\\" + InfoDatabase.GetCollection("Info").FindOne(x => true)["Exe"].AsString);
                var aaa = @"""" + b + @""" " + arg;
                string appData = ApplicationData.Current.LocalCacheFolder.Path;
                inParameters["CommandLine"] = aaa;
                inParameters["ProcessStartupInformation"] = processInfo;

                var result = managementClass.InvokeMethod("Create", inParameters, null);
                InfoDatabase.Dispose();

                var returnStart = (uint)result.Properties["ReturnValue"].Value;
                if ((result != null) && returnStart == 0 && returnStart != 9)
                {
                    var NewProcess = Process.GetProcessById(Convert.ToInt32(result.Properties["ProcessId"].Value));

                    RunningApps.Add(AppID, NewProcess);
                    return true;
                }
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
    public static async Task<bool> CloseInstalledApp(ObjectId AppID)
    {
        try
        {
            RunningApps.TryGetValue(AppID, out var app);
            app?.Kill();
            RunningApps.Remove(AppID);
            return true;
        }
        catch { return false; }
    }
}