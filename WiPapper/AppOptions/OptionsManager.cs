using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WiPapper.AppOptions
{
    public static class OptionsManager
    {
        public static Options Options = new Options(); //Публичное статическое поле, представляющее экземпляр класса Options, который содержит настройки приложения.

        // My Documents
        private static string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //Приватное статическое поле, содержащее путь к "Мои документы".
        private static string FilePath = MyDocuments + "\\WiPapper\\Options.json"; //Приватное статическое поле, содержащее полный путь к файлу Options.xml, который используется для сохранения и загрузки настроек.

        public static void InitializeOptions() //Итак, если сохраненные настройки доступны, они загружаются, иначе устанавливаются значения настроек по умолчанию. Это позволяет вашему приложению работать с настройками, независимо от того, есть ли у пользователя сохраненные настройки или нет.
        {
            if (!DeserializeOptions())
            {
                AssignDefaults();
            }
        }

        public static bool SerializeOptions() //Публичный статический метод для сохранения настроек в XML-файл. Возвращает true в случае успешного сохранения, иначе false.
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(FilePath))) //Проверка существования директории, в которой будет храниться файл настроек. Если директории нет, то она создается.
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                }

                using (FileStream fstream = new FileStream(FilePath, FileMode.Create)) //Открытие FileStream для записи в файл Options.xml. Файл будет создан или перезаписан (если существует).
                {
                    // Сериализуем объект в JSON и записываем в файл    //Сериализация объекта Options и запись его в файл. Сериализация(грубо говоря архивирование)
                    JsonSerializer.Serialize(fstream, Options, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch (Exception) //Если произошло исключение при сохранении настроек, метод возвращает false.
            {
                return false;
            }
            return true; //return true;: Если сохранение прошло успешно, метод возвращает true.
        }

        private static bool DeserializeOptions() //Этот метод служит для загрузки сохраненных настроек из файла XML. Если файл настроек существует и может быть успешно прочитан и десериализован, метод возвращает true, и настройки будут доступны для использования в вашем приложении. В противном случае он возвращает false, и в приложении могут использоваться значения настроек по умолчанию или другая логика по умолчанию для настроек.
        {
            if (!File.Exists(FilePath)) { return false; } //Проверка существования файла с настройками. Если файл отсутствует, возвращается false.

            try
            {
                using (FileStream reader = new FileStream(FilePath, FileMode.Open)) //Открытие FileStream для чтения из файла Options.xml.
                {
                    Options = JsonSerializer.Deserialize<Options>(reader);  // Десериализация содержимого файла в объект Options.
                                                                            //мы ожидаем десериализацию JSON-данных в объект типа Settings
                }
            }
            catch (Exception ex) //Если произошло исключение при загрузке настроек, выводится сообщение об ошибке в консоль, и метод возвращает false.
            {
                Console.WriteLine("!! Error loading Options.xml");
                Console.WriteLine(ex.Message);
                return false;
            }
            return true; //Если загрузка настроек прошла успешно, метод возвращает true.
        }

        private static void AssignDefaults() //Приватный статический метод AssignDefaults для установки значений настроек по умолчанию.
        {
            Options.Settings = new OptionsSettings(); //Создание нового экземпляра OptionsSettings и установка его в свойство Settings объекта Options.

            Options.StartMinimized = false;
            Options.SetWallpapperWhenLaunched = false;
            Options.StartWhenLaunched = false;
            Options.UseDifferentSettingsWhenMaximized = false;
            Options.StartWithWindows = false;

            Options.Settings.MainTaskbarStyle = new OptionsSettingsMainTaskbarStyle(); //Создание нового экземпляра OptionsSettingsMainTaskbarStyle и установка его в свойство MainTaskbarStyle объекта Options.
            Options.Settings.MainTaskbarStyle.AccentState = 3;
            Options.Settings.MainTaskbarStyle.GradientColor = "#804080FF";
            Options.Settings.MainTaskbarStyle.Colorize = true;
            Options.Settings.MainTaskbarStyle.UseWindowsAccentColor = true;
            Options.Settings.MainTaskbarStyle.WindowsAccentAlpha = 127;

            Options.Settings.MaximizedTaskbarStyle = new OptionsSettingsMaximizedTaskbarStyle(); //Создание нового экземпляра OptionsSettingsMaximizedTaskbarStyle и установка его в свойство MaximizedTaskbarStyle объекта Options.
            Options.Settings.MaximizedTaskbarStyle.AccentState = 2;
            Options.Settings.MaximizedTaskbarStyle.GradientColor = "#FF000000";
            Options.Settings.MaximizedTaskbarStyle.Colorize = false;
            Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor = true;
            Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha = 255;
        }
    }
}
