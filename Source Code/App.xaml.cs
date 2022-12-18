using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace RCC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        readonly List<Tuple<string, string>> list_fonts = new List<Tuple<string, string>>
        {
            Tuple.Create(@"Font Awesome 6 Brands-Regular-400.otf", "https://github.com/Midoruya/rust-cheat-checker/blob/main/Resources/Font%20Awesome%206%20Brands-Regular-400.otf?raw=true"),
            Tuple.Create(@"Font Awesome 6 Free-Regular-400.otf", "https://github.com/Midoruya/rust-cheat-checker/blob/main/Resources/Font%20Awesome%206%20Free-Regular-400.otf?raw=true"),
            Tuple.Create(@"Font Awesome 6 Free-Solid-900.otf", "https://github.com/Midoruya/rust-cheat-checker/blob/main/Resources/Font%20Awesome%206%20Free-Solid-900.otf?raw=true"),
        };
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool is_admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!is_admin)
            {
                System.Windows.Forms.MessageBox.Show("Obavezno pokrenuti kao Administrator!");
                Environment.Exit(Environment.ExitCode);
            }
            string get_file_version_from_git = new WebClient().DownloadString("https://raw.githubusercontent.com/Midoruya/rust-cheat-checker/main/version.ini");
            string get_file_version_from_assembly = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            if (get_file_version_from_git.Equals(get_file_version_from_assembly) == false)
            {
                DialogResult button_pressed = System.Windows.Forms.MessageBox.Show(
                       "Nova verzija je izašla želite li ažurirati?",
                       "Ažuriranje",
                       MessageBoxButtons.YesNo,
                       MessageBoxIcon.Information,
                       MessageBoxDefaultButton.Button1,
                       System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
                if (button_pressed == DialogResult.Yes)
                {
                    new WebClient().DownloadFile($"https://github.com/Midoruya/rust-cheat-checker/releases/download/{get_file_version_from_git}/LSCC.exe", "Updated.exe");
                    ProcessStartInfo start_info = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C timeout 5 & del LSCC.exe & move Updated.exe LSCC.exe & del Updated.exe & runas LSCC.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process.Start(start_info);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            Thread thread = new Thread(() =>
            {
                detecting_cleaning detecting = new detecting_cleaning();
                detecting.search_all();
            });
            Thread font_thread = new Thread(() => list_fonts.ForEach(fonts => new Thread(() =>
            {
                try
                {
                    string font_path = $"C:\\Windows\\Temp\\{fonts.Item1}";
                    new WebClient().DownloadFile(fonts.Item2, fonts.Item1);
                    all_dll_import.AddFontResource(font_path);
                }
                catch { /* The current file uses another process */ }
            }).Start()));
            main_window main = new main_window();
            thread.Start();
            font_thread.Start();
            main.Show();
        }
    }
}
