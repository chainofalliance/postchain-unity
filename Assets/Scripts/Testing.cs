using Microsoft.Win32;
using UnityEngine;
using System.Linq;
using System.Text;

public class Testing
{
    public static void Register()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat(
            "{0}/{1}.exe", System.IO.Directory.GetParent(Application.dataPath), Application.productName
        );
        sb.Replace("/", "\\");
        var keyTest = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
        RegistryKey key = keyTest.CreateSubKey("unity");
        key.SetValue("URL Protocol", "");
        key.CreateSubKey(@"shell\open\command").SetValue("", @$"""{sb.ToString()}""" + "-batchmode -nographics" + @" ""%1""");
    }
}
