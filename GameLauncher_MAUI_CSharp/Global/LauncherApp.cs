using LiteDB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WebSocketSharp;

public static class LauncherApp
{
    public static string test;
    public static string GITHUB_CLIENT_ID = "945ac4987b3311821363";
    public static string GITHUB_CLIENT_SECRETS = "c076183289fa6989aec355335e60bfc5bdb4737f";
    public static string redirect_uri = "x-open-laucher://oauth";
    public static string AppPath = AppContext.BaseDirectory + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe";
    public static readonly LiteDatabase db = new LiteDatabase(GetDataBasePath());
    public static string GetAppDataDir()
    {
        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(
  Environment.SpecialFolder.ApplicationData), "XOpenLauncher\\")))
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(
  Environment.SpecialFolder.ApplicationData), "XOpenLauncher\\"));
        }
        return Path.Combine(Environment.GetFolderPath(
  Environment.SpecialFolder.ApplicationData), "XOpenLauncher\\");
    }
    public static string GetDataBasePath()
    {
        return Path.Combine(GetAppDataDir(), "app.db");
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
}