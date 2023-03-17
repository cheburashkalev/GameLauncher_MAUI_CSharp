using Microsoft.AspNetCore.Components.WebView.Maui;
using GameLauncher_MAUI_CSharp.Data;
using System.Net.Http;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Win32;
using url_scheme_manager;
using GameLauncher_MAUI_CSharp.WinUI;
using static System.Net.Mime.MediaTypeNames;
using GameLauncher_MAUI_CSharp.Code.TorrentLib;
using Octokit;
using GameLauncher_MAUI_CSharp.Shared.Layout;




#if WINDOWS
using Colors = Microsoft.UI.Colors;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Windows.UI.ViewManagement;
using WebSocketSharp;
#endif


namespace GameLauncher_MAUI_CSharp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{

        //LauncherApp.test = string.Concat(Environment.GetCommandLineArgs());
        if (Environment.GetCommandLineArgs().Length > 1)
        {
            LauncherApp.TryConnecntAndSendNewArg(Environment.GetCommandLineArgs()[1],false);
            LauncherApp.test = Environment.GetCommandLineArgs()[1];
        }
        LauncherApp.TryConnecntAndSendNewArg("",true);
        LauncherApp.StartServer();
        var cl = LauncherApp.db.GetCollection<DB_OAuth>("OAuth");
        if (cl.Exists(x => x.AuthServise == "GitHub") )
        {if(cl.FindOne(x => x.AuthServise == "GitHub").token != null)
            TorrentDownloader.client.Credentials = new Credentials(cl.FindOne(x => x.AuthServise == "GitHub").token);
        }
            var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
        builder.Services.AddScoped<BlazorTransitionableRoute.IRouteTransitionInvoker, RouteTransitionInvoker>();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<HttpClient>();

        builder.ConfigureLifecycleEvents(events =>
         {

 #if WINDOWS10_0_19041_0_OR_GREATER
             events.AddWindows(wndLifeCycleBuilder =>
             {                
                 wndLifeCycleBuilder.OnWindowCreated(window =>
                 {
                     IntPtr nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                     WindowId nativeWindowId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
                     var uiSettings = new Windows.UI.ViewManagement.UISettings();
                     var color = uiSettings.GetColorValue(UIColorType.Accent);
                     AppWindow appWindow = AppWindow.GetFromWindowId(nativeWindowId);
                     var presenter = appWindow.Presenter as OverlappedPresenter;
                     /* window.ExtendsContentIntoTitleBar = true;
                      presenter.IsMaximizable = false;
                      presenter.IsMinimizable = false;
                      presenter.IsAlwaysOnTop = true;
                      presenter.IsResizable = true;


                      var titleBar = appWindow.TitleBar;
                      appWindow.TitleBar.BackgroundColor = color;
                      titleBar.ForegroundColor = Colors.White;
                      titleBar.BackgroundColor = Colors.Green;
                      titleBar.ButtonForegroundColor = Colors.White;
                      titleBar.ButtonBackgroundColor = Colors.SeaGreen;
                      titleBar.ButtonHoverForegroundColor = Colors.Gainsboro;
                      titleBar.ButtonHoverBackgroundColor = Colors.DarkSeaGreen;
                      titleBar.ButtonPressedForegroundColor = Colors.Gray;
                      titleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

                      // Set inactive window colors
                      titleBar.InactiveForegroundColor = Colors.Gainsboro;
                      titleBar.InactiveBackgroundColor = Colors.SeaGreen;
                      titleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
                      titleBar.ButtonInactiveBackgroundColor = Colors.SeaGreen;
                      presenter.SetBorderAndTitleBar(false, false);*/



                     window.Title = "Aboba Launcher";
                     

                     //  ResizeMode = "CanResizeWithGrip" AllowsTransparency = "True"
                     //window.CenterOnScreen(400, 750);
                     //p.SetBorderAndTitleBar(false, false);
                });
            });
#endif
        });


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
		
		builder.Services.AddSingleton<WeatherForecastService>();

		return builder.Build();
	}
}
