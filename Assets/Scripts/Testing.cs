using Microsoft.Win32;

public class Testing
{
    public static void Register()
    {
        var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var keyTest = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
        RegistryKey key = keyTest.CreateSubKey("simon");
        key.SetValue("URL Protocol", "wnPing");
        key.CreateSubKey(@"shell\open\command").SetValue("", @loc + " %1");
    }
}
