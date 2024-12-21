using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SAMPQuery;
using BLauncher.ViewModel;
using BLauncher.View;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;
using System.IO.Compression;
using Microsoft.Win32;
using System.Threading;
using System.Text.RegularExpressions;

namespace BLauncher
{
    public partial class MainWindow : Window
    {
        private static string SERVER_IP = Settings.SERVER_IP;
        private bool checkAndPlay = false;
        private const string registryKeyBanderstadt = "HKEY_CURRENT_USER\\SOFTWARE\\BanderstadtProject";
        private CancellationTokenSource _cancellationTokenSource;
        public static SampQuery api = new SampQuery(SERVER_IP);

        public MainWindow()
        {
            InitializeComponent();
            EnsureRegistryKey();
            StartUpdatingServerInfo();
        }

        private void InitializeServerInfo()
        {
            try
            {
                var data = api.GetServerInfo();
                hostname.Text = data.HostName;
                players.Text = $"{data.Players} / {data.MaxPlayers}";
                ping.Text = $"{data.ServerPing} ms";
                if(data.ServerPing > 150)
                {
                    ChangePingColor(Colors.Red);
                } 
                else if(data.ServerPing >= 90 && data.ServerPing <= 150) {
                    ChangePingColor(Colors.Orange);
                }
                else
                {
                    ChangePingColor((Color)ColorConverter.ConvertFromString("#209E24"));
                }
                hostavailability.Fill = data.Password
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A4141B"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#209E24"));
            }
            catch
            {
                hostname.Text = "BANDERSTADT PROJECT | Технічні роботи";
                players.Text = "0/0";
                ping.Text = "0 ms";
                ChangePingColor(Colors.Red);
                hostavailability.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A4141B"));
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)FindResource("MinimizeStoryboard");
            sb.Completed += (s, a) => WindowState = WindowState.Minimized;
            sb.Begin();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                var restoreStoryboard = (Storyboard)FindResource("RestoreStoryboard");
                restoreStoryboard.Begin();
            }
        }

        private void SiteButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://banderstadt.pp.ua");
        private void ForumButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://forum.banderstadt.pp.ua");
        private void DonateButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://banderstadt.pp.ua/donate");
        private void TelegramButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://t.me/banderstadt_gta");
        private void InstagramButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://www.instagram.com/banderstadt.gta/");
        private void DiscordButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://discord.gg/BBXsNCcgnV");
        private void YoutubeButton_Click(object sender, RoutedEventArgs e) => OpenUrl("https://www.youtube.com/@Banderstadt-GTA");
        private void ButtonNews_Click(object sender, RoutedEventArgs e) => OpenUrl("https://banderstadt.pp.ua/news");

        private static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!checkAndPlay)
            {
                await CheckAndDownloadGame();
            }
            else
            {
                StartGame();
                //GameLauncher.LaunchGame(NicknameTextBox.Text, IP_CONNECT, 7777);
            }
        }

        private void NicknameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Hidden;
            NicknameTextBox.CaretBrush = Brushes.Black;
        }

        private async Task CheckAndDownloadGame()
        {
            if (CheckGameInstallation())
            {
                DisplayStatus("У Вас актуальна версія. Приємної гри :)", true);
                checkAndPlay = true;
            }
            else
            {
                DownloadPanel.Visibility = Visibility.Visible;
                StatusMessage.Visibility = Visibility.Collapsed;
                await DownloadAndExtractGameFiles();
            }
        }

        private bool CheckGameInstallation()
        {
            string gameFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin/Game/gta_sa.exe");
            string binDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (!Directory.Exists(binDirectory)) Directory.CreateDirectory(binDirectory);

            return File.Exists(gameFilePath);
        }

        private async Task DownloadAndExtractGameFiles()
        {
            string remoteArchivePath = "/home/ftpuser/files/Game.zip";
            string localArchivePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game.zip");
            string extractionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            var sftpClient = new SftpClient(Settings.SFTP_SERVER, 38444, Settings.SFTP_USER, Settings.SFTP_PASSWORD);
            sftpClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(30);
            sftpClient.OperationTimeout = TimeSpan.FromMinutes(5);
            sftpClient.Connect();

            long totalSize = sftpClient.GetAttributes(remoteArchivePath).Size;

            using (var fileStream = File.Create(localArchivePath))
            {
                await Task.Run(() => sftpClient.DownloadFile(remoteArchivePath, fileStream, bytesTransferred =>
                {
                    UpdateProgress(bytesTransferred, totalSize);
                }));
            }

            sftpClient.Disconnect();

            await ExtractArchive(localArchivePath, extractionPath);
            DisplayStatus("Встановлення завершено. Приємної гри :)", true);
            checkAndPlay = true;
            if (File.Exists(localArchivePath)) File.Delete(localArchivePath);
        }

        private void UpdateProgress(ulong bytesTransferred, long totalSize)
        {
            Dispatcher.Invoke(() =>
            {
                double progressPercentage = (double)bytesTransferred / totalSize * 100;
                DownloadProgressBar.Value = progressPercentage;
                FileSizeText.Text = $"{bytesTransferred / (1024 * 1024)} MB / {totalSize / (1024 * 1024)} MB";
                PercentageText.Text = $"{progressPercentage:F0}%";
            });
        }
        
        private void NicknameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NicknameTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
            NicknameTextBox.CaretBrush = Brushes.Transparent;
        }

        private void NicknameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(NicknameTextBox.Text) ?
                 Visibility.Visible :
                 Visibility.Hidden;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var launcherUpdates = new LauncherUpdates();
            launcherUpdates.CheckAndUpdate();
        }

        private async Task ExtractArchive(string archivePath, string extractionPath)
        {
            DisplayStatus("Відбувається встановлення гри. Зачекайте хвилинку...", true);
            await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, extractionPath));
        }

        private async void StartGame()
        {
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            var gtaexe = Path.Combine(exePath, "bin/Game/gta_sa.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(exePath, "bin/Game/gta_sa.exe"),
                Arguments = $"-c -n {NicknameTextBox.Text} -h {SERVER_IP} -p 7777 -loadsamp",
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(exePath, "bin/Game")
            };

            if (IsValidNickname(NicknameTextBox.Text))
            {
                    Registry.SetValue(registryKeyBanderstadt, "PlayerName", $"{NicknameTextBox.Text}");
                    Process process = Process.Start(startInfo);
                    await Task.Delay(2000);
            }
            else
            {
                MessageBox.Show("Нікнейм повинен містити від 3 до 20 символів і лише літери або цифри!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }    
        }

        private bool IsValidNickname(string nickname)
        {
            return Regex.IsMatch(nickname, @"^[A-Za-z0-9]{3,20}$");
        }

        public void EnsureRegistryKey()
        {
            using (RegistryKey? currentUserKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", writable: true))
            {
                if (currentUserKey == null) return;

                using (RegistryKey? banderstadtKey = currentUserKey.OpenSubKey("BanderstadtProject", writable: true))
                {
                    if (banderstadtKey == null)
                    {
                        using (RegistryKey? sampKey = currentUserKey.OpenSubKey("SAMP", writable: true))
                        {
                            if (sampKey != null && sampKey.GetValue("PlayerName") is string playerName)
                            {
                                NicknameTextBox.Text = playerName;
                                using (RegistryKey newBanderstadtKey = currentUserKey.CreateSubKey("BanderstadtProject"))
                                {
                                    newBanderstadtKey.SetValue("PlayerName", NicknameTextBox.Text, RegistryValueKind.String);
                                }
                            }
                            else
                            {
                                using (RegistryKey newBanderstadtKey = currentUserKey.CreateSubKey("BanderstadtProject"))
                                {
                                    newBanderstadtKey.SetValue("PlayerName", NicknameTextBox.Text, RegistryValueKind.String);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (banderstadtKey.GetValue("PlayerName") is string playerName)
                        {
                            NicknameTextBox.Text = playerName;
                        }
                        else
                        {
                            banderstadtKey.SetValue("PlayerName", NicknameTextBox.Text, RegistryValueKind.String);
                        }
                    }
                }
            }
        }

        private async void StartUpdatingServerInfo()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await Dispatcher.InvokeAsync(() => InitializeServerInfo());
                    await Task.Delay(5000);
                }
            }, token);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            base.OnClosing(e);
        }

        private void DisplayStatus(string message, bool hideDownloadPanel)
        {
            StatusMessage.Text = message;
            StatusMessage.Visibility = Visibility.Visible;
            DownloadPanel.Visibility = hideDownloadPanel ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ChangePingColor(Color newColor)
        {
            var brush = new SolidColorBrush(newColor);

            PingRect1.Fill = brush;
            PingRect2.Fill = brush;
            PingRect3.Fill = brush;
            PingRect4.Fill = brush;
        }
    }
}

