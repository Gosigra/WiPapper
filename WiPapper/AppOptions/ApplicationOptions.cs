namespace WiPapper.AppOptions
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Options
    {
        private bool _startMinimizedField;

        private bool _setWallpapperWhenLaunched;

        private bool _startWhenLaunchedField;

        private bool _useDifferentSettingsWhenMaximizedField;

        private bool _startWithWindowsField;

        private byte _chooseAFitComboBoxIndex;

        private OptionsSettings _settingsField;

        public bool StartMinimized
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

        public OptionsSettings Settings 
        {
            get => this._settingsField;
            set => this._settingsField = value;
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OptionsSettings
    {
        private string _wallpapperPath; 

        private string _defaultInstallationPath; 

        private OptionsSettingsMainTaskbarStyle _mainTaskbarStyleField;

        private OptionsSettingsMaximizedTaskbarStyle _maximizedTaskbarStyleField;

        public string WallpapperPath
        {
            get => this._wallpapperPath;
            set => this._wallpapperPath = value;
        }

        public string DefaultInstallationPath
        {
            get => this._defaultInstallationPath;
            set => this._defaultInstallationPath = value;
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

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettingsMainTaskbarStyle
    {
        private byte _accentStateField;

        private string _gradientColorField;

        private bool _colorizeField;

        private bool _useWindowsAccentColorField;

        private byte _windowsAccentAlphaField;

        public byte AccentState
        {
            get => this._accentStateField;
            set
            {
                this._accentStateField = value;
                Taskbars.UpdateAccentState();
            }
        }

        public string GradientColor
        {
            get => this._gradientColorField;
            set
            {
                this._gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        public bool Colorize
        {
            get => this._colorizeField;
            set
            {
                this._colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        public bool UseWindowsAccentColor
        {
            get => this._useWindowsAccentColorField;
            set
            {
                this._useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        public byte WindowsAccentAlpha
        {
            get => this._windowsAccentAlphaField;
            set
            {
                this._windowsAccentAlphaField = value;
                Taskbars.UpdateColor();
            }
        }
    }

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

        public byte AccentState
        {
            get => this._accentStateField;
            set
            {
                this._accentStateField = value;
                Taskbars.UpdateAccentState();
            }
        }

        public string GradientColor
        {
            get => this._gradientColorField;
            set
            {
                this._gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }
        public bool Colorize
        {
            get => this._colorizeField;
            set
            {
                this._colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }
        public bool UseWindowsAccentColor
        {
            get => this._useWindowsAccentColorField;
            set
            {
                this._useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        public byte WindowsAccentAlpha
        {
            get => this._windowsAccentAlphaField;
            set
            {
                this._windowsAccentAlphaField = value;
                Taskbars.UpdateColor();
            }
        }
    }
}
