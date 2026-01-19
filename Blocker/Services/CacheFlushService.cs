using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Blocker.Services;

public class CacheFlushService : ICacheFlushService
{
    public void Flush()
    {
        ProcessStartInfo procStart;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            procStart = new ProcessStartInfo("ipconfig", "/flushdns");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            procStart = new ProcessStartInfo("resolvectl", "flush-caches");
        }
        else
        {
            throw new Exception("Cannot flush DNS cache - unsupported operating system");
        }
        
        procStart.UseShellExecute = false;
        procStart.RedirectStandardOutput = true;
        procStart.RedirectStandardError = true;
        var proc = Process.Start(procStart);
        if (proc is null)
            throw new Exception("proc is null");
        proc.WaitForExit();
    }
}