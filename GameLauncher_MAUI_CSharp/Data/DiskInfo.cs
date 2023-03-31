using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher_MAUI_CSharp.Data
{
    internal class DiskInfo
    {
        public string? Name { get; set; }
        public string? RootDirectory { get; set; }
        public long SizeBytes { get; set; }
        public long AvailableSpaceBytes { get; set; }
    }
}
