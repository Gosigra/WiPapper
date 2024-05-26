using System;
using System.IO;
using System.Text.Json;

namespace WiPapper
{
    public static class TaskBarOptions
    {
        public static Options Options = new Options(); //Публичное статическое поле, представляющее экземпляр класса Options, который содержит настройки приложения.

        // My Documents
        private static string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //Приватное статическое поле, содержащее путь к "Мои документы".
        private static string FilePath = MyDocuments + "\\WiPapper\\Options.json"; //Приватное статическое поле, содержащее полный путь к файлу Options.xml, который используется для сохранения и загрузки настроек.

        public static void InitializeOptions() //Итак, если сохраненные настройки доступны, они загружаются, иначе устанавливаются значения настроек по умолчанию. Это позволяет вашему приложению работать с настройками, независимо от того, есть ли у пользователя сохраненные настройки или нет.
        {
            if (!LoadOptions())
            {
                AssignDefaults();
            }
        }

        public static bool SaveOptions() //Публичный статический метод для сохранения настроек в XML-файл. Возвращает true в случае успешного сохранения, иначе false.
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

        private static bool LoadOptions() //Этот метод служит для загрузки сохраненных настроек из файла XML. Если файл настроек существует и может быть успешно прочитан и десериализован, метод возвращает true, и настройки будут доступны для использования в вашем приложении. В противном случае он возвращает false, и в приложении могут использоваться значения настроек по умолчанию или другая логика по умолчанию для настроек.
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
            Options.Settings.SetWallpapperWhenLaunched = false;
            Options.Settings.StartMinimized = false;
            Options.Settings.StartWhenLaunched = false;
            Options.Settings.StartWithWindows = false;
            Options.Settings.UseDifferentSettingsWhenMaximized = false;

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

    [System.SerializableAttribute()] // Атрибут, указывающий, что класс Options может быть сериализован. Этот атрибут может использоваться фреймворком для определения того, как обрабатывать этот класс при сериализации и десериализации.
    [System.ComponentModel.DesignerCategoryAttribute("code")] //Атрибут, указывающий категорию, к которой относится класс в дизайнере.
    public partial class Options // Объявление класса Options, который представляет настройки. //partical не надо как будто
    {
        private OptionsSettings settingsField; //Приватное поле settingsField для хранения объекта OptionsSettings.

        private string wallpapperPath;

        /// <remarks/>
        public OptionsSettings Settings //Свойство для доступа к объекту 
        {
            get
            {
                return this.settingsField;
            }
            set
            {
                this.settingsField = value;
            }


        }

        public string WallpapperPath
        {
            get
            {
                return this.wallpapperPath;
            }
            set
            {
                this.wallpapperPath = value;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OptionsSettings
    {
        private byte chooseAFitComboBoxIndex;

        private bool setWallpapperWhenLaunched;

        private bool startMinimizedField;

        private bool startWhenLaunchedField;

        private bool startWithWindowsField;

        private bool useDifferentSettingsWhenMaximizedField;

        private OptionsSettingsMainTaskbarStyle mainTaskbarStyleField; // Приватное поле для хранения объекта поднастроек основного стиля панели задач.

        private OptionsSettingsMaximizedTaskbarStyle maximizedTaskbarStyleField;

        public byte ChooseAFitComboBoxIndex
        {
            get
            {
                return this.chooseAFitComboBoxIndex;
            }
            set
            {
                this.chooseAFitComboBoxIndex = value;
            }
        }

        public bool SetWallpapperWhenLaunched
        {
            get
            {
                return this.setWallpapperWhenLaunched;
            }
            set
            {
                this.setWallpapperWhenLaunched = value;
            }
        }

        public bool StartMinimized //Свойство для доступа к параметру "StartMinimized". Возвращает startMinimizedField и устанавливает его значение.
        {
            get
            {
                return this.startMinimizedField;
            }
            set
            {
                this.startMinimizedField = value;
            }
        }

        public bool StartWhenLaunched
        {
            get
            {
                return this.startWhenLaunchedField;
            }
            set
            {
                this.startWhenLaunchedField = value;
            }
        }

        public bool StartWithWindows
        {
            get
            {
                return this.startWithWindowsField;
            }
            set
            {
                this.startWithWindowsField = value;
            }
        }

        public bool UseDifferentSettingsWhenMaximized
        {
            get
            {
                return this.useDifferentSettingsWhenMaximizedField;
            }
            set
            {
                this.useDifferentSettingsWhenMaximizedField = value;
            }
        }

        public OptionsSettingsMainTaskbarStyle MainTaskbarStyle
        {
            get
            {
                return this.mainTaskbarStyleField;
            }
            set
            {
                this.mainTaskbarStyleField = value;
            }
        }

        public OptionsSettingsMaximizedTaskbarStyle MaximizedTaskbarStyle
        {
            get
            {
                return this.maximizedTaskbarStyleField;
            }
            set
            {
                this.maximizedTaskbarStyleField = value;
            }
        }
    }

    [System.SerializableAttribute()]  // Аналогично
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettingsMainTaskbarStyle //Объявление класса OptionsSettingsMainTaskbarStyle, представляющего поднастройки основного стиля панели задач.
    {

        private byte accentStateField;

        private string gradientColorField;

        private bool colorizeField;

        private bool useWindowsAccentColorField;

        private byte windowsAccentAlphaField;

        /// <remarks/>
        public byte AccentState //Свойство для доступа к параметру "AccentState". Возвращает accentStateField и устанавливает его значение, вызывая Taskbars.UpdateAccentState() при изменении.
        {
            get
            {
                return this.accentStateField;
            }
            set
            {
                this.accentStateField = value;
                Taskbars.UpdateAccentState();
            }

        }

        /// <remarks/>
        public string GradientColor //Аналогично
        {
            get
            {
                return this.gradientColorField;
            }
            set
            {
                this.gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public bool Colorize //Аналогично
        {
            get
            {
                return this.colorizeField;
            }
            set
            {
                this.colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        /// <remarks/>
        public bool UseWindowsAccentColor //Аналогично
        {
            get
            {
                return this.useWindowsAccentColorField;
            }
            set
            {
                this.useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public byte WindowsAccentAlpha //Аналогично
        {
            get
            {
                return this.windowsAccentAlphaField;
            }
            set
            {
                this.windowsAccentAlphaField = value;
                Taskbars.UpdateColor(); //if (UseWindowsAccentColor) { Taskbars.UpdateColor(); }  //zdifcfhgvxdflgxfgkdkfhcvhncfdfhddfxff
            }
        }
    }


    //Аналогично
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettingsMaximizedTaskbarStyle
    {

        private byte accentStateField;

        private string gradientColorField;

        private bool colorizeField;

        private bool useWindowsAccentColorField;

        private byte windowsAccentAlphaField;

        /// <remarks/>
        public byte AccentState
        {
            get
            {
                return this.accentStateField;
            }
            set
            {
                this.accentStateField = value;
                Taskbars.UpdateAccentState();
            }
        }

        /// <remarks/>
        public string GradientColor
        {
            get
            {
                return this.gradientColorField;
            }
            set
            {
                this.gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public bool Colorize
        {
            get
            {
                return this.colorizeField;
            }
            set
            {
                this.colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        /// <remarks/>
        public bool UseWindowsAccentColor
        {
            get
            {
                return this.useWindowsAccentColorField;
            }
            set
            {
                this.useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public byte WindowsAccentAlpha
        {
            get
            {
                return this.windowsAccentAlphaField;
            }
            set
            {
                this.windowsAccentAlphaField = value;
                Taskbars.UpdateColor(); //if (UseWindowsAccentColor) { Taskbars.UpdateColor();}
            }
        }
    }
}
