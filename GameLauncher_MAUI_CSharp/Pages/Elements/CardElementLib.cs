using GameLauncher_MAUI_CSharp.Code.TorrentLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher_MAUI_CSharp.Pages.Elements
{
    public class CardEventArgs : EventArgs
    {
    }
    public class CardElementLib
    {
        public static EventHandler<TokenReciveEventArgs>? TokenRecive;
    }
}
