using System;
using System.IO;
using System.Text.Json;

namespace WiPapper.AppOptions
{
    [System.SerializableAttribute()] // Атрибут, указывающий, что класс Options может быть сериализован. Этот атрибут может использоваться фреймворком для определения того, как обрабатывать этот класс при сериализации и десериализации.
    [System.ComponentModel.DesignerCategoryAttribute("code")] //Атрибут, указывающий категорию, к которой относится класс в дизайнере.
    public partial class Options // Объявление класса Options, который представляет настройки. //partical не надо как будто
    {
        private bool _startMinimizedField;

        private bool _setWallpapperWhenLaunched;

        private bool _startWhenLaunchedField;

        private bool _useDifferentSettingsWhenMaximizedField; // это опции приложения

        private bool _startWithWindowsField;

        private byte _chooseAFitComboBoxIndex;

        private OptionsSettings _settingsField; //Приватное поле settingsField для хранения объекта OptionsSettings.

        public bool StartMinimized //Свойство для доступа к параметру "StartMinimized". Возвращает startMinimizedField и устанавливает его значение.
        {
            get => this._startMinimizedField;
            set => this._startMinimizedField = value;
        }
        public bool SetWallpapperWhenLaunched
        {
            get => this._setWallpapperWhenLaunched;
            set => this._setWallpapperWhenLaunched = value;
        }

        public bool StartWhenLaunched
        {
            get => this._startWhenLaunchedField; 
            set => this._startWhenLaunchedField = value;
        }

        public bool UseDifferentSettingsWhenMaximized
        {
            get => this._useDifferentSettingsWhenMaximizedField; 
            set => this._useDifferentSettingsWhenMaximizedField = value;
        }

        public bool StartWithWindows
        {
            get => this._startWithWindowsField;
            set => this._startWithWindowsField = value;
        }

        public byte ChooseAFitComboBoxIndex
        {
            get => this._chooseAFitComboBoxIndex;
            set => this._chooseAFitComboBoxIndex = value;
        }

        /// <remarks/>
        public OptionsSettings Settings //Свойство для доступа к объекту 
        {
            get => this._settingsField;
            set => this._settingsField = value;
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OptionsSettings
    {
        private string _wallpapperPath; //добавить громкость чтобы сохранялась 

        private OptionsSettingsMainTaskbarStyle _mainTaskbarStyleField; // Приватное поле для хранения объекта поднастроек основного стиля панели задач.

        private OptionsSettingsMaximizedTaskbarStyle _maximizedTaskbarStyleField;

        public string WallpapperPath
        {
            get => this._wallpapperPath;
            set => this._wallpapperPath = value;
        }

        public OptionsSettingsMainTaskbarStyle MainTaskbarStyle
        {
            get => this._mainTaskbarStyleField;
            set => this._mainTaskbarStyleField = value;
        }

        public OptionsSettingsMaximizedTaskbarStyle MaximizedTaskbarStyle
        {
            get => this._maximizedTaskbarStyleField;
            set => this._maximizedTaskbarStyleField = value;
        }
    }

    [System.SerializableAttribute()]  // Аналогично
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettingsMainTaskbarStyle //Объявление класса OptionsSettingsMainTaskbarStyle, представляющего поднастройки основного стиля панели задач.
    {
        private byte _accentStateField;

        private string _gradientColorField;

        private bool _colorizeField;

        private bool _useWindowsAccentColorField;

        private byte _windowsAccentAlphaField;

        /// <remarks/>
        public byte AccentState //Свойство для доступа к параметру "AccentState". Возвращает accentStateField и устанавливает его значение, вызывая Taskbars.UpdateAccentState() при изменении.
        {
            get => this._accentStateField;
            set
            {
                this._accentStateField = value;
                Taskbars.UpdateAccentState();
            }

        }

        /// <remarks/>
        public string GradientColor //Аналогично
        {
            get => this._gradientColorField;
            set
            {
                this._gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public bool Colorize //Аналогично
        {
            get => this._colorizeField;
            set
            {
                this._colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        /// <remarks/>
        public bool UseWindowsAccentColor //Аналогично
        {
            get => this._useWindowsAccentColorField;
            set
            {
                this._useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public byte WindowsAccentAlpha //Аналогично
        {
            get => this._windowsAccentAlphaField;
            set
            {
                this._windowsAccentAlphaField = value;
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
        private byte _accentStateField;

        private string _gradientColorField;

        private bool _colorizeField;

        private bool _useWindowsAccentColorField;

        private byte _windowsAccentAlphaField;

        /// <remarks/>
        public byte AccentState
        {
            get => this._accentStateField;
            set
            {
                this._accentStateField = value;
                Taskbars.UpdateAccentState();
            }
        }

        /// <remarks/>
        public string GradientColor
        {
            get => this._gradientColorField;
            set
            {
                this._gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public bool Colorize
        {
            get => this._colorizeField;
            set
            {
                this._colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        /// <remarks/>
        public bool UseWindowsAccentColor
        {
            get => this._useWindowsAccentColorField;
            set
            {
                this._useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public byte WindowsAccentAlpha
        {
            get => this._windowsAccentAlphaField;
            set
            {
                this._windowsAccentAlphaField = value;
                Taskbars.UpdateColor(); //if (UseWindowsAccentColor) { Taskbars.UpdateColor();}
            }
        }
    }
}
