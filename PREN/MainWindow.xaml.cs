using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace PREN
{
    public partial class MainWindow : Window
    {
        private string logFilePath = "Log.txt";
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RunSw_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sisteminizi optimize etmek istiyor musunuz?", "PREN", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Optimizasyon baþladý!");

                string tempPath = Path.GetTempPath();
                var dir = new DirectoryInfo(tempPath);

                if (!dir.Exists)
                {
                    MessageBox.Show("Temp klasörünü bulunamadý.");
                    return;
                }

                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                ProgressTextBlock.Text = "Sistem optimize ediliyor...";
                await Task.Run(() => OptimizeSystem());

                ProgressTextBlock.Dispatcher.Invoke(() =>
                {
                    ProgressTextBlock.Text = "Geçici klasör temizleniyor...";
                });

                await Task.Run(() => ClearTempFolder(dir));

                if (MessageBox.Show("Deðiþiklikleri uygulamak için bilgisayarý yeniden baþlatmanýz gerekir. Þimdi yeniden baþlatmak istiyor musunuz?", "PREN", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    ProgressTextBlock.Dispatcher.Invoke(() =>
                    {
                        ProgressTextBlock.Text = "Bilgisayar yeniden baþlatýlýyor...";
                    });
                    await Task.Run(() => RestartComputer());
                }
                else
                {
                    MessageBox.Show("Deðiþiklikleri uygulamak için bilgisayarý daha sonra yeniden baþlatýn!");
                }

                ProgressBar.Dispatcher.Invoke(() =>
                {
                    ProgressBar.Visibility = Visibility.Hidden;
                });
                ProgressTextBlock.Dispatcher.Invoke(() =>
                {
                    ProgressTextBlock.Text = "";
                });
                CreateLogFile();
            }
        }
        private void CreateLogFile()
        {
            string logContent = $"Optimizasyon tamamlandý: {DateTime.Now}\n";
            logContent += $"- Geçici klasör temizlendi\n";
            logContent += $"- Sistem optimize edildi\n";
            logContent += $"- Bilgisayar yeniden baþlatýldý\n";

            try
            {
                File.WriteAllText(logFilePath, logContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Günlük dosyasý oluþturulurken hata oluþtu: " + ex.Message, "HATA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearTempFolder(DirectoryInfo dir)
        {
            double totalItems = dir.GetDirectories().Length + dir.GetFiles().Length;
            double progress = 0;

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                try
                {
                    subDir.Delete(true);
                }
                catch (Exception ex)
                {
                    LogError("Alt dizin silinirken hata oluþtu: " + ex.Message);
                }

                progress++;
                UpdateProgressBar(progress / totalItems, $"Alt dizinler siliniyor... {progress}/{totalItems}");
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    LogError("EDosya silinirken hata oluþtu: " + ex.Message);
                }

                progress++;
                UpdateProgressBar(progress / totalItems, $"Dosyalar siliniyor... {progress}/{totalItems}");
            }
        }

        private void OptimizeSystem()
        {
            try
            {
                Optimize.Optimizing();
            }
            catch (Exception ex)
            {
                LogError("Sistem optimize edilirken hata oluþtu: " + ex.Message);
            }

            UpdateProgressBar(1, "Optimizasyon tamamlandý!");
        }

        private void RestartComputer()
        {
            try
            {
                Process.Start("shutdown.exe", "-r -t 00");
            }
            catch (Exception ex)
            {
                LogError("Bilgisayar yeniden baþlatýlýrken hata oluþtu: " + ex.Message);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SupportMe_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/decimaldreamer/PREN");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Optimize.ConvertToNormal();
            if (MessageBox.Show("Deðiþiklikleri geri almak için bilgisayarý yeniden baþlatmanýz gerekir. Þimdi yeniden baþlatmak istiyor musunuz?", "PREN", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                RestartComputer();
            }
        }

        private void services_Click(object sender, RoutedEventArgs e)
        {
            Services sv = new Services();
            sv.Show();
        }

        private void LogError(string errorMessage)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("ErrorLog.txt", true))
                {
                    writer.WriteLine(errorMessage + " " + DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata günlüðüne yazma hatasý: " + ex.Message, "HATA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProgressBar(double progress, string progressText)
        {
            progress = Math.Max(0, Math.Min(progress * 100, 100));
            ProgressBar.Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = progress;
            });
            ProgressTextBlock.Dispatcher.Invoke(() =>
            {
                ProgressTextBlock.Text = progressText;
            });
        }
    }
}
