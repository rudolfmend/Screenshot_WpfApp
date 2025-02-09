using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using ProgressBar = System.Windows.Controls.ProgressBar;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Screenshot_WpfApp
{
    public partial class MainWindow : Window
    {
        private const string APP_VERSION = "2.6.0";
        private const string APP_NAME = "Screenshot App";
        private const string SETTINGS_FILE = "settings.json";
        private AppSettings settings = new();

        public MainWindow()
        {
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            InitializeComponent();
            LoadSettings();
            EnsureDefaultDirectoryExists();
        }

        private void LoadSettings()
        {
            if (File.Exists(SETTINGS_FILE))
            {
                try
                {
                    string json = File.ReadAllText(SETTINGS_FILE);
                    var loadedSettings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
                    if (loadedSettings != null)
                    {
                        settings.DefaultSaveDirectory = loadedSettings.DefaultSaveDirectory;
                        settings.DefaultFileNamePrefix = loadedSettings.DefaultFileNamePrefix;
                        settings.DefaultFileFormat = loadedSettings.DefaultFileFormat;
                        settings.LastUsedNumber = loadedSettings.LastUsedNumber;
                    }
                }
                catch
                {
                    // If loading fails, we'll use default settings
                    SaveSettings(); // Save default settings
                }
            }
            else
            {
                SaveSettings(); // Save default settings
            }
        }

        private void SaveSettings()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(settings);
            File.WriteAllText(SETTINGS_FILE, json);
        }

        private string GenerateFileName()
        {
            string baseFileName;
            string fullPath;
            int number = settings.LastUsedNumber;

            // Zabezpečíme, že máme platný formát
            string fileFormat = settings.DefaultFileFormat;
            if (string.IsNullOrEmpty(fileFormat) || !fileFormat.StartsWith("."))
            {
                fileFormat = ".png";
            }

            do
            {
                number++;
                baseFileName = $"{settings.DefaultFileNamePrefix}{number:D5}";
                fullPath = Path.Combine(settings.DefaultSaveDirectory, baseFileName + fileFormat);
            } while (File.Exists(fullPath));

            settings.LastUsedNumber = number;
            SaveSettings();

            return fullPath;
        }

        private void EnsureDefaultDirectoryExists()
        {
            if (!Directory.Exists(settings.DefaultSaveDirectory))
            {
                try
                {
                    Directory.CreateDirectory(settings.DefaultSaveDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create default directory: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            System.Threading.Thread.Sleep(250);
            try
            {
                Screen primaryScreen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
                using (Bitmap screenshot = new Bitmap(
                    primaryScreen.Bounds.Width,
                    primaryScreen.Bounds.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(screenshot))
                    {
                        graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
                    }

                    Microsoft.Win32.SaveFileDialog saveDialog = new()
                    {
                        Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp",
                        Title = "Save Screenshot",
                        FileName = Path.GetFileName(GenerateFileName()),
                        InitialDirectory = settings.DefaultSaveDirectory, // Use folder from settings
                        DefaultExt = settings.DefaultFileFormat.TrimStart('.')// Use default format from settings
                    };

                    // Set default filter according to selected format
                    switch (settings.DefaultFileFormat.ToLower())
                    {
                        case ".jpg":
                            saveDialog.FilterIndex = 2; // Index for JPEG in Filter string
                            break;
                        case ".bmp":
                            saveDialog.FilterIndex = 3; // Index for BMP in Filter string
                            break;
                        default:
                            saveDialog.FilterIndex = 1; // PNG is the default
                            break;
                    }

                    if (saveDialog.ShowDialog() == true)
                    {
                        string ext = Path.GetExtension(saveDialog.FileName).ToLower();
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                        switch (ext)
                        {
                            case ".jpg":
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                format = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                        }
                        screenshot.Save(saveDialog.FileName, format);

                        // Updating "LastUsedNumber" based on the really correct and up-to-date file used
                        string numberPart = Path.GetFileNameWithoutExtension(saveDialog.FileName)
                            .Replace(settings.DefaultFileNamePrefix, "");
                        if (int.TryParse(numberPart, out int number))
                        {
                            settings.LastUsedNumber = number;
                            SaveSettings();
                        }
                    }
                }
            }
            finally
            {
                this.Show();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(settings);
            if (settingsWindow.ShowDialog() == true)
            {
                settings = settingsWindow.GetSettings();
                SaveSettings();
            }
        }

        //---Menu items---
        private void RateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Open the rating link in the browser (change the URL to correct one)
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://your-app-rating-url.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open rating page: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VersionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var buildOnlyDate = File.GetCreationTime(System.Reflection.Assembly.GetExecutingAssembly().Location)
                .ToString("d"); // "d" displays short date format - without time

            MessageBox.Show(
                $"{APP_NAME}\nVersion: {APP_VERSION}\n" +
                $"Build date (day.month.year): {buildOnlyDate}",
                "Version Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show(
                    $"Current version: {APP_VERSION}\n\n" +
                    "To check for updates, visit our website or Microsoft Store.",
                    "Update Check",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                var result = MessageBox.Show(
                    "Would you like to visit Microsoft Store to check for updates?",
                    "Visit Store",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Otvorí Microsoft Store (nahraďte YOUR_STORE_ID vaším skutočným Store ID)
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "ms-windows-store://pdp/?productid=YOUR_STORE_ID",
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        // Záložný plán - otvorí webový prehliadač
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://your-website.com",
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to check for updates: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // Pomocná trieda pre zobrazenie progressu
        public class ProgressDialog : Window
        {
            public ProgressDialog(string message)
            {
                WindowStyle = WindowStyle.None;
                Width = 300;
                Height = 100;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;

                var grid = new Grid();
                Content = grid;

                var progressBar = new ProgressBar
                {
                    IsIndeterminate = true,
                    Margin = new Thickness(20),
                    Height = 20
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                grid.Children.Add(progressBar);
                grid.Children.Add(textBlock);
            }
        }


        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"{APP_NAME}\n" +
                "Version: " + APP_VERSION + "\n\n" +
                "This application allows you to easily capture and save screenshots.\n\n" +
                "Features:\n" +
                "- Full screen capture\n" +
                "- Automatic file naming\n" +
                "- Customizable save location\n" +
                "- Multiple image formats support\n\n" +
                "© 2024 Rudolf Mendzezof\n" +
                "All rights reserved.",
                "About " + APP_NAME,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UninstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to uninstall the application?\n" +
                "This will remove the application and all its settings.",
                "Confirm Uninstall",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Remove settings file
                    if (File.Exists(SETTINGS_FILE))
                    {
                        File.Delete(SETTINGS_FILE);
                    }

                    // Inform user about the next steps
                    MessageBox.Show(
                        "Settings have been removed.\n\n" +
                        "To complete the uninstallation:\n" +
                        "1. Close this application\n" +
                        "2. Go to Windows Settings\n" +
                        "3. Select 'Apps'\n" +
                        "4. Find '" + APP_NAME + "' and click Uninstall",
                        "Uninstall Instructions",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // End the application
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to uninstall: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(settings.DefaultSaveDirectory))
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", settings.DefaultSaveDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open folder: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The default save directory does not exist. Taking a screenshot will create it.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }        
    }
}
