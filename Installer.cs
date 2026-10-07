using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class Installer
{
    private const string AppName = "AC Kill XP";
    private const string Version = "0.1.0";
    private const string PluginGuid = "{4EDC248A-E237-4516-A17C-5099F515ADC9}";
    private const string Surrogate = "{71A69713-6593-47EC-0002-0000000DECA1}";
    private const string InstallDir = @"C:\Games\Decal Plugins\AC Kill XP";

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
                MessageBox.Show("Please close Asheron's Call before installing AC Kill XP.", AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string decal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"Decal 3.0\Decal.Adapter.dll");

            if (!File.Exists(decal))
            {
                MessageBox.Show("Decal 3.0 was not found. Install Decal first, then run this setup again.", AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Directory.CreateDirectory(InstallDir);
            ExtractResource("ACKillXP.dll", Path.Combine(InstallDir, "ACKillXP.dll"));
            ExtractResource("uninstall.exe", Path.Combine(InstallDir, "uninstall.exe"));
            ExtractResource("README.txt", Path.Combine(InstallDir, "README.txt"));

            using (RegistryKey plugin = Registry.LocalMachine.CreateSubKey(@"Software\Decal\Plugins\" + PluginGuid))
            {
                plugin.SetValue("", AppName, RegistryValueKind.String);
                plugin.SetValue("Enabled", 1, RegistryValueKind.DWord);
                plugin.SetValue("Object", "ACKillXP.PluginCore", RegistryValueKind.String);
                plugin.SetValue("Assembly", "ACKillXP.dll", RegistryValueKind.String);
                plugin.SetValue("Path", InstallDir, RegistryValueKind.String);
                plugin.SetValue("Surrogate", Surrogate, RegistryValueKind.String);
                plugin.SetValue("Uninstaller", AppName, RegistryValueKind.String);
            }

            using (RegistryKey un = Registry.LocalMachine.CreateSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppName))
            {
                un.SetValue("DisplayName", AppName, RegistryValueKind.String);
                un.SetValue("DisplayVersion", Version, RegistryValueKind.String);
                un.SetValue("Publisher", "EWARAC", RegistryValueKind.String);
                un.SetValue("InstallLocation", InstallDir, RegistryValueKind.String);
                un.SetValue("UninstallString", "\"" + Path.Combine(InstallDir, "uninstall.exe") + "\"", RegistryValueKind.String);
                un.SetValue("DisplayIcon", Path.Combine(InstallDir, "uninstall.exe"), RegistryValueKind.String);
                un.SetValue("NoModify", 1, RegistryValueKind.DWord);
                un.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }

            MessageBox.Show("AC Kill XP " + Version + " is installed and enabled in Decal.\n\nStart Asheron's Call normally.",
                AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Installation failed:\n\n" + ex.Message, AppName,
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

    private static void ExtractResource(string resourceName, string destination)
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using (Stream input = asm.GetManifestResourceStream(resourceName))
        {
            if (input == null)
                throw new InvalidOperationException("Installer resource missing: " + resourceName);

            using (FileStream output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                input.CopyTo(output);
        }
    }
}
