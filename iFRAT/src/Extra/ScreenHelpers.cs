using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace iFRAT.Extra;
public sealed partial class ScreenHelpers
{
    public enum SystemMetric
    {
        VirtualScreenWidth = 78, // CXVIRTUALSCREEN 0x0000004E 
        VirtualScreenHeight = 79, // CYVIRTUALSCREEN 0x0000004F 
    }

    [LibraryImport("user32.dll")]
    public static partial int GetSystemMetrics(SystemMetric metric);

    public static Size GetVirtualDisplaySize()
    {
        var width = GetSystemMetrics(SystemMetric.VirtualScreenWidth);
        var height = GetSystemMetrics(SystemMetric.VirtualScreenHeight);

        return new Size(width, height);
    }
}