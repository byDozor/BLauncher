using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SAMPQuery;
using BLauncher.ViewModel;
using System.Diagnostics;

namespace BLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string IP_CONNECT = (string)Settings.ip;
        public MainWindow()
        {
            try
            {
                SampQuery api = new SampQuery(IP_CONNECT);
                ServerInfo data = api.GetServerInfo();

                InitializeComponent();
                hostname.Text = data.HostName;
                players.Text = data.Players + " / " + data.MaxPlayers;
                ping.Text = data.ServerPing + " ms";
                if(data.Password)
                {
                    hostavailability.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A4141B"));
                }
            }
            catch
            {
                //MessageBox.Show(e.Message);
                InitializeComponent();
                hostname.Text = "BANDERSTADT PROJECT | Технічні роботи";
                players.Text = "0/0";
                ping.Text = "0 ms";
                hostavailability.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A4141B"));
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Hidden;
        }

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private void MyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderText.Visibility = Visibility.Hidden;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Метод для згортання вікна
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (Storyboard)FindResource("MinimizeStoryboard");
            sb.Completed += (s, a) =>
            {
                // Після завершення анімації згорнути вікно
                this.WindowState = WindowState.Minimized;
            };
            sb.Begin();
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                // Виконати анімацію відновлення
                Storyboard restoreStoryboard = (Storyboard)FindResource("RestoreStoryboard");
                restoreStoryboard.Begin();
            }
        }

        private void SiteButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://banderstadt.pp.ua", 
                UseShellExecute = true
            });
        }

        private void ForumButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://forum.banderstadt.pp.ua", 
                UseShellExecute = true
            });
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://banderstadt.pp.ua/donate", 
                UseShellExecute = true
            });
        }

        private void TelegramButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/banderstadt_gta", 
                UseShellExecute = true
            });
        }

        private void InstagramButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.instagram.com/banderstadt.gta/", 
                UseShellExecute = true
            });
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/BBXsNCcgnV",
                UseShellExecute = true
            });
        }

        private void YoutubeButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/@Banderstadt-GTA", 
                UseShellExecute = true
            });
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("В розробці");
        }
    }
}

