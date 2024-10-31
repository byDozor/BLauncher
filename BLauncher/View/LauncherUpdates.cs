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
        string SFTP_SERVER = (string)Settings.SFTP_SERVER;
        string SFTP_USER = (string)Settings.SFTP_USER;
        string SFTP_PASSWORD = (string)Settings.SFTP_PASSWORD;
        string curVersion = (string)Settings.currentVersion;

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

        private bool IsNewVersion(string currentVersion, string latestVersion)
        {
            return new Version(latestVersion).CompareTo(new Version(currentVersion)) > 0;
        }

        public async void CheckAndUpdate()
        {
            var (latestVersion, url) = await CheckForUpdates();
            if (IsNewVersion(curVersion, latestVersion))
            {
                MessageBox.Show($"[BETA]\nВийшла нова версія лаунчеру: {latestVersion}\nВстановлена версія:{curVersion}\nОновлення буде встановлено автоматично\nНатисніть ОК щоб продовжити");
                string tempUpdatePath = Path.Combine(Path.GetTempPath(), "BLauncherUpdate");
                await DownloadUpdate(url, tempUpdatePath);
            }
            else
            {
                MessageBox.Show($"[BETA]\nУ вас встановлена актуальна версія лаунчеру {curVersion}");
            }
        }
        private async Task DownloadUpdate(string url, string tempDirectory)
        {
            string host = SFTP_SERVER; // замініть на свій домен
            string username = SFTP_USER;
            string password = SFTP_PASSWORD;
            int port = 38444;
            string remoteFilePath = "/home/ftpuser/files/Launcher/update.zip"; // повний шлях до файлу на сервері
            string localFilePath = Path.Combine(tempDirectory, "update.zip"); // зберігає в тимчасовій папці

            try
            {
                using (var sftp = new SftpClient(host, port, username, password))
                {
                    // Підключаємося до SFTP-сервера
                    sftp.Connect();
                    // Створюємо тимчасову директорію, якщо її ще не існує
                    Directory.CreateDirectory(tempDirectory);
                    // Завантажуємо файл
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        await Task.Run(() => sftp.DownloadFile(remoteFilePath, fileStream));
                    }
                    // Розпаковуємо завантажений архів у тимчасову директорію
                    System.IO.Compression.ZipFile.ExtractToDirectory(localFilePath, tempDirectory, true);
                    // Відключаємося від SFTP-сервера
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
