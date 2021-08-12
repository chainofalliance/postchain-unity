using Microsoft.Win32;
using System.Text;

using System.Diagnostics;
using System;


public class Testing
{
    public static void Register()
    {
        var location = new StringBuilder(Process.GetCurrentProcess().MainModule.FileName);
        location.Replace("/", "\\ ");

        var keyTest = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
        RegistryKey key = keyTest.CreateSubKey("unity");
        key.SetValue("URL Protocol", "");
        key.CreateSubKey(@"shell\open\command").SetValue("", @$"""{location.ToString()}""" + "-batchmode -nographics" + @" ""%1""");
    }

    public static void RegisterUnix()
    {
        var location = new StringBuilder(Process.GetCurrentProcess().MainModule.FileName);
        location.Replace(" ", "\\ ");

        var desktopEntry = new StringBuilder();
        desktopEntry.AppendFormat(
            "[Desktop Entry]\nName={0}\nExec={1} -batchmode -nographics %u\nType=Application\nTerminal=false\nMimeType=x-scheme-handler/{0}",
            SSOStandalone.SCHEME, location.ToString()
        );

        var addDesktopFile = new StringBuilder();
        addDesktopFile.AppendFormat(
            "echo '{0}' > ~/.local/share/applications/{1}.desktop", desktopEntry.ToString(), SSOStandalone.SCHEME
        );

        var mimeAppEntry = new StringBuilder();
        mimeAppEntry.AppendFormat("x-scheme-handler/{0}={0}.desktop", SSOStandalone.SCHEME);

        var updateMimeList = new StringBuilder();
        updateMimeList.AppendFormat(
            "echo {0} >> ~/.local/share/applications/mimeapps.list", mimeAppEntry.ToString()
        );

        var containsEntry = new StringBuilder();
        containsEntry.AppendFormat(
            "grep -Fx '{0}' ~/.local/share/applications/mimeapps.list", mimeAppEntry.ToString()
        );

        addDesktopFile.ToString().Bash();
        var hasEntry = containsEntry.ToString().Bash();
        if (String.IsNullOrEmpty(hasEntry))
        {
            updateMimeList.ToString().Bash();
            "update-desktop-database ~/.local/share/applications".Bash();
        }
    }
}

public static class ShellHelper
{
    public static string Bash(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return result;
    }
}