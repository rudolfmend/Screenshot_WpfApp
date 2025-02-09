using System.Windows;
using System.Windows.Controls;

namespace Screenshot_WpfApp
{
    public partial class SettingsWindow : Window
    {
        private AppSettings settings;

        public SettingsWindow(AppSettings currentSettings)
        {
            InitializeComponent();
            settings = new AppSettings
            {
                DefaultSaveDirectory = currentSettings.DefaultSaveDirectory,
                DefaultFileNamePrefix = currentSettings.DefaultFileNamePrefix,
                DefaultFileFormat = currentSettings.DefaultFileFormat,
                LastUsedNumber = currentSettings.LastUsedNumber
            };

            // Set correct item in ComboBox
            foreach (ComboBoxItem item in FormatComboBox.Items)
            {
                if (item.Tag?.ToString() == settings.DefaultFileFormat)
                {
                    FormatComboBox.SelectedItem = item;
                    break;
                }
            }
            DataContext = settings;
        }

        public AppSettings GetSettings()
        {
            if (FormatComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is string format)
            {
                settings.DefaultFileFormat = format;
            }
            else
            {
                settings.DefaultFileFormat = ".png"; // Default value if an error occurs
            }
            return settings;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                InitialDirectory = settings.DefaultSaveDirectory,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.DefaultSaveDirectory = dialog.SelectedPath;
                DirectoryTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
