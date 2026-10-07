using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class Uninstaller
{
    private const string AppName = "AC Kill XP";
    private const string PluginGuid = "{4EDC248A-E237-4516-A17C-5099F515ADC9}";

    [STAThread]
    private static void Main()
    {
        try
        {
            if (!IsAdministrator())
            {
                RelaunchElevated();
                return;
            }

            if (Process.GetProcessesByName("acclient").Length > 0)
            {
                MessageBox.Show("Please close Asheron's Call before removing AC Kill XP.", AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\Decal\Plugins\" + PluginGuid, false); } catch { }
            try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppName, false); } catch { }

            string dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string dll = Path.Combine(dir, "ACKillXP.dll");
            string readme = Path.Combine(dir, "README.txt");

            try { if (File.Exists(dll)) File.Delete(dll); } catch { }
            try { if (File.Exists(readme)) File.Delete(readme); } catch { }

            string cmd = "/c ping 127.0.0.1 -n 2 > nul & rmdir /s /q \"" + dir + "\"";
            Process.Start(new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                Arguments = cmd,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Uninstall failed:\n\n" + ex.Message, AppName,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static bool IsAdministrator()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(id);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void RelaunchElevated()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
    }
}
