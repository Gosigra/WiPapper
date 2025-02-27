using System;
using System.IO;
using System.Text.Json;

namespace WiPapper.AppOptions
{
    public static class OptionsManager
    {
        public static Options Options = new Options();

        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string FilePath = MyDocuments + "\\WiPapper\\Options.json";

        public static void InitializeOptions()
        {
            if (!DeserializeOptions())
            {
                AssignDefaults();
            }
        }

        public static bool SerializeOptions()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                }

                using (FileStream fstream = new FileStream(FilePath, FileMode.Create))
                {
                    JsonSerializer.Serialize(fstream, Options, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool DeserializeOptions()
        {
            if (!File.Exists(FilePath)) { return false; }

            try
            {
                using (FileStream reader = new FileStream(FilePath, FileMode.Open))
                {
                    Options = JsonSerializer.Deserialize<Options>(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка загрузки настроек из Options.json");
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        private static void AssignDefaults()
        {
            Options.Settings = new OptionsSettings();

            Options.StartMinimized = false;
            Options.SetWallpapperWhenLaunched = false;
            Options.StartWhenLaunched = false;
            Options.UseDifferentSettingsWhenMaximized = false;
            Options.StartWithWindows = false;

            Options.Settings.MainTaskbarStyle = new OptionsSettingsMainTaskbarStyle
            {
                AccentState = 3,
                GradientColor = "#804080FF",
                Colorize = true,
                UseWindowsAccentColor = true,
                WindowsAccentAlpha = 127
            };

            Options.Settings.MaximizedTaskbarStyle = new OptionsSettingsMaximizedTaskbarStyle
            {
                AccentState = 2,
                GradientColor = "#FF000000",
                Colorize = false,
                UseWindowsAccentColor = true,
                WindowsAccentAlpha = 255
            };
        }
    }
}
