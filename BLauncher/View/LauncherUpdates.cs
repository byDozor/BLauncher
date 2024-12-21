using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using BLauncher.ViewModel;
using Newtonsoft.Json;
using Renci.SshNet;

namespace BLauncher.View
{
    class LauncherUpdates
    {
        private string SFTP_SERVER = Settings.SFTP_SERVER;
        private string SFTP_USER = Settings.SFTP_USER;
        private string SFTP_PASSWORD = Settings.SFTP_PASSWORD;
        private int SFTP_PORT = 38444;

        string curVersion = GetLocalProductVersion();

        //string CuttenVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);

        private async Task<(string version, string url)> CheckForUpdates()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("http://banderstadt.pp.ua/launcher_version.json");
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                return (data["version"], data["url"]);
            }
        }

        public static string GetLocalProductVersion()
        {
            return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion ?? "1.0.0";
        }

        /*private bool IsNewVersion(string currentVersion, string latestVersion)
        {
            return new Version(latestVersion).CompareTo(new Version(currentVersion)) > 0;
        }*/
        private bool IsNewVersion(string currentVersion, string latestVersion)
        {
            if (Version.TryParse(latestVersion, out Version? latest) &&
                Version.TryParse(currentVersion, out Version? current))
            {
                return latest.CompareTo(current) > 0;
            }
            else
            {
                MessageBox.Show($"Невірний формат версії: {latestVersion}");
                return false;
            }
        }

        public async void CheckAndUpdate()
        {
            var (latestVersion, url) = await CheckForUpdates();
            if (IsNewVersion(curVersion, latestVersion))
            {
                MessageBox.Show($"[BETA]\nВийшла нова версія лаунчеру: {latestVersion}\nВстановлена версія:{curVersion}\nОновлення буде встановлено автоматично\nНатисніть ОК щоб продовжити");
                string tempUpdatePath = Path.Combine(Path.GetTempPath(), "BLauncherUpdate");
                if (Directory.Exists(tempUpdatePath))
                {
                    Directory.Delete(tempUpdatePath, true);
                }
                await DownloadUpdate(url, tempUpdatePath);
            }
            else
            {
                MessageBox.Show($"[BETA]\nУ вас встановлена актуальна версія лаунчеру {curVersion}");
            }
        }
        private async Task DownloadUpdate(string url, string tempDirectory)
        {
            string remoteFilePath = "/home/ftpuser/files/Launcher/update.zip"; // повний шлях до файлу на сервері
            string localFilePath = Path.Combine(tempDirectory, "update.zip");

            try
            {
                using (var sftp = new SftpClient(SFTP_SERVER, SFTP_PORT, SFTP_USER, SFTP_PASSWORD))
                {
                    sftp.Connect();
                    Directory.CreateDirectory(tempDirectory);
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        await Task.Run(() => sftp.DownloadFile(remoteFilePath, fileStream));
                    }

                    System.IO.Compression.ZipFile.ExtractToDirectory(localFilePath, tempDirectory, true);
                    sftp.Disconnect();

                    if (File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                    }

                    string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");
                    Process.Start(updaterPath, $"\"{AppDomain.CurrentDomain.BaseDirectory}BLauncher.exe\" \"{tempDirectory}\"");
                    Application.Current.Shutdown();
                }
            } 
            catch {
                MessageBox.Show($"[BETA]\nВиникла помилка\nНе вдалось оновити лаунчер\nСпробуйте пізніше або зверніться до розробників.");
            }
        }
    }
}
