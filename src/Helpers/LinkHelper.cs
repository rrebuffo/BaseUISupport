using System;
using System.Diagnostics;

namespace BaseUISupport.Helpers;

public static class LinkHelper
{
    public static void Open(string url)
    {
        string uri = new Uri(url).AbsoluteUri;
        ProcessStartInfo info = new(uri) { UseShellExecute = true };
        Process.Start(info);
    }
}
