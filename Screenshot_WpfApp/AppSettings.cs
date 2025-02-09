using System.IO;

namespace Screenshot_WpfApp
{
    public class AppSettings
    {
        private string defaultSaveDirectory;
        public const string DEFAULT_FOLDER_NAME = "ScreenshotsApp_storage";

        public AppSettings()
        {
            string myPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            defaultSaveDirectory = Path.Combine(myPictures, DEFAULT_FOLDER_NAME);
            DefaultFileFormat = ".png"; // Explicitne nastavíme predvolenú hodnotu
        }

        public string DefaultSaveDirectory
        {
            get => defaultSaveDirectory;
            set => defaultSaveDirectory = value;
        }
        public string DefaultFileNamePrefix { get; set; } = "screenshot";
        public string DefaultFileFormat { get; set; }
        public int LastUsedNumber { get; set; } = 0;
    }
}
