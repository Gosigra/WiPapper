using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Vanara.PInvoke;
using System.Windows.Interop;
using static WiPapper.Globals;
using Extensions;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using WiPapper.Wallpaper.HtmlWallpaper;

//Получать высоту панели задач и передавать в браузер её + цвет панели чтобы на сайте можно было сделать визуализацию как будто от панели задач столбцы(подумал что можно без высоты и чтобы разработчики сами её писали)


//using static Vanara.PInvoke.User32;
//using static Vanara.PInvoke.Gdi32;

namespace WiPapper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public static string file; //
        public static Uri fileMedia;
        //public static Window win2;  //не используется вроде
        public static MediaElement media = null;
                
        public static List<Window> windowList;
        public static List<Rectangle> ScreenList;

        public static HWND workerw;
        //public static HWND workerwHidden; //

        //public static MediaPlayer player; //

        //public static bool soundOrNot = false; //
        public static bool currentlyPlaying = false;


        #region Declarations
        // Main window initialization 
        bool WindowInitialized = false;  // Флаг инициализации окна
        string MyPath = Assembly.GetExecutingAssembly().Location;       // Путь к исполняемому файлу

        // Taskbars // Объявление задачи ApplyTask и флага RunApplyTask
        static Task ApplyTask;
        static bool RunApplyTask = false;
        public static bool FindTaskbarHandles = true;


        private static bool alphaDragStarted = false;

        // Explorer restarts and Windows Accent Colour changes 
        // Коды сообщений для перезапуска проводника и изменения цвета акцента Windows
        private static readonly uint WM_TASKBARCREATED = User32.RegisterWindowMessage("TaskbarCreated");

        // Window state hook 
        // Хук для отслеживания изменения состояния окна
        private static User32.WinEventProc procDelegate = new User32.WinEventProc(WinEventProc);
        private static User32.HWINEVENTHOOK WindowStateHook;
        private static HWND LastClosedWindow;
        private static DateTime LastClosedWindowTime;

        // Start with Windows registry key
        // Реестровый ключ для запуска с Windows
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true); //     Registry.CurrentUser = HKEY_CURRENT_USER

        #endregion Declarations


        NotifyIcon ni;

        public MainWindow()
        {
            InitializeComponent();

            SetHtmlWallpaper.StartHttpListener();


            ni = new NotifyIcon();                                                                                                     // Инициализация иконки системного трея
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("Resources/1.ico", UriKind.Relative)).Stream;
            ni.Icon = new Icon(iconStream);           
            ni.Click += (object sender, EventArgs args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };


            /*
            * Check monitor numbers and put them in differents Arraylist  
            * // проверка кол-ва мониторов и помещение в разные массивы
            * 
            * ScreenList is used to get the actual resolution of a screen 
            * //ScreenList используется для получения фактического разрешения экрана.
            * 
            * windowList will be used to create 1 window per monitor *Not yet implemented*  
            * //windowList будет использоваться для создания 1 окна на монитор *еще не реализовано*
            * 
            * ButtonList will be used to create dynamically button in order to select and load a file for each monitor *Not yet implemented*  
            * //ButtonList будет использоваться для динамического создания кнопки для выбора и загрузки файла для каждого монитора *Пока не реализовано*
            */

            windowList = new List<Window>();
            ScreenList = new List<Rectangle>();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                ScreenList.Add(Screen.AllScreens[i].Bounds);                                                                                                 //получаю рабочий стол и его рабочую область ({X = 0 Y = 0 Width = 1920 Height = 1040}) //working area
            }

            for (int i = 0; i < ScreenList.Count; i++)
            {
                windowList.Add(new Window());                                                                                                                //здесь кол-во экранов
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                ni.BalloonTipTitle = "WiPaper";
                ni.BalloonTipText = "WiPaper было свёрнуто";
                ni.Visible = true;
                ni.ShowBalloonTip(10000);
                //ni.ShowBalloonTip(10000, "WiPaper", "WiPaper было свёрнуто", ToolTipIcon.Info);
                Hide();
            }
            else if (WindowState.Normal == this.WindowState)
            {
                ni.Visible = false;
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ni.Visible = false;

            if (media != null)
            {
                media.Stop();
                media = null;
                for (int i = 0; i < windowList.Count; i++)
                {
                    windowList[i].Close();
                }
            }
            
            base.OnClosing(e);
        }

        public static void FindWorkerWindow()
        {
            HWND progMan = User32.FindWindow("ProgMan", null);

            IntPtr result = IntPtr.Zero;

            User32.SendMessageTimeout(progMan, 0x052C, new IntPtr(0), IntPtr.Zero, User32.SMTO.SMTO_NORMAL, 1000, ref result);

            workerw = IntPtr.Zero;

            User32.EnumWindows(new User32.EnumWindowsProc((tophandle, fdhg) =>
            {
                HWND p = User32.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (p != IntPtr.Zero)
                {
                    workerw = User32.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                }

                return true;
            }),IntPtr.Zero );
        }

        private void SelectWall_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "All files(*.*)|*.*";

            var rezult = fileDialog.ShowDialog();

            switch (rezult)
            {
                case System.Windows.Forms.DialogResult.OK:
                    //file = fileDialog.FileName;
                    fileMedia = new Uri(fileDialog.FileName);
                    SetWallpaperButton.IsEnabled = true;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                    break;
                default:
                    SetWallpaperButton.IsEnabled = false;
                    break;

            }
        }

        private void SetWallpaperButton_Click(object sender, RoutedEventArgs e)
        {
            FindWorkerWindow();

            //if (media != null)
            if (currentlyPlaying == true)
            {
                UnSetWall();
            }

            for (int i = 0; i < windowList.Count; i++) // можно цикл убрать и сделать для 1 монитора иначе потом для не 1го монитора надо асинхронность
            {
                windowList[i] = new Window();

                windowList[i].WindowStyle = WindowStyle.None;
                windowList[i].AllowsTransparency = true;

                windowList[i].Top = ScreenList[i].Top;
                windowList[i].Left = ScreenList[i].Left;
                windowList[i].Width = ScreenList[i].Width;
                windowList[i].Height = ScreenList[i].Height;

                windowList[i].Initialized += new EventHandler((s, ea) => // тут надо зарефакторить media- это для медиа, а для html надо cefsharp 
                {
                    
                    if (fileMedia.AbsolutePath.Contains("index.html"))
                    {
                        //метод 1 - (сделать проверки для всякого (например нужно ли запись включать и тд)+ можно асинхронность но потом под конец(сначало главное чтобы работало))
                        SetHtmlWallpaper.FilePath = Path.GetDirectoryName(fileMedia.LocalPath);
                        SetHtmlWallpaper.SetBrowserAsWallpaper(windowList[i]);
                        
                        currentlyPlaying = true;
                    }
                    else
                    {
                        //метод 2-в него то что ниже
                        SetMediaAsWallpaper2(windowList[i]);
                    }

                    HWND windowHandle = new WindowInteropHelper(windowList[i]).Handle;
                    User32.SetParent(windowHandle, workerw);
                });
                windowList[i].UpdateLayout();
                windowList[i].Show();
                WallpaperStretchTypeComboBox_SelectionChanged(null, null);
            }
        }

        private void SetMediaAsWallpaper2(Window windowList)
        {
            media = new MediaElement();
            Grid grid = new Grid();
            windowList.Content = grid;
            grid.Children.Add(media);

            media.Source = fileMedia;
            media.LoadedBehavior = MediaState.Manual;  //454532542254
            media.Volume = 0;

            media.MediaEnded += (send, eArgs) =>
            {
                media.Position = new TimeSpan(0, 0, 1);
                media.Play();
            };
            media.Play();
            currentlyPlaying = true;
        }

        private void WallpaperStretchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!WindowInitialized || media == null) return;
            media.Stretch = (Stretch)WallpaperStretchTypeComboBox.SelectedIndex;
        }

        private void UnSetWallpaper_Click(object sender, RoutedEventArgs e)
        {
            UnSetWall();
        }

        private void UnSetWall()
        {
            if (currentlyPlaying == true)
            {
                media?.Stop();
                media = null;
                for (int i = 0; i < windowList.Count; i++)
                {
                    windowList[i].Close();
                }

                currentlyPlaying = false;
            }
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentlyPlaying == true)
                media.Volume = Volume.Value;            
        }
























        #region Initializations
        private void PopulateComboBoxes() //Этот метод PopulateComboBoxes() служит для заполнения элемента управления ComboBox (в данном случае, AccentStateComboBox) значениями перечисления AccentState
        {

            AccentComboBox.ItemsSource = Enum.GetValues(typeof(AccentState)).Cast<AccentState>();
            FakeAccentComboBox.SelectedIndex = 0;
            AccentComboBox.SelectedIndex = 0;

            //ChooseAFitComboBox.SelectedIndex = 0;
        }

        private void LoadSettings() //Этот метод позволяет загрузить сохраненные настройки и отразить их в интерфейсе вашего приложения, чтобы пользователь мог видеть текущие значения настроек.
        {
            TaskBarOptions.InitializeOptions();

            SwitchTaskbarBeingEdited("Main");

            SetWallpapperWhenLaunchedCheckBox.IsChecked = TaskBarOptions.Options.Settings.SetWallpapperWhenLaunched;
            StartMinimizedCheckBox.IsChecked = TaskBarOptions.Options.Settings.StartMinimized;
            StartWhenLaunchedCheckBox.IsChecked = TaskBarOptions.Options.Settings.StartWhenLaunched;
            UseMaximizedSettingsCheckBox.IsChecked = TaskBarOptions.Options.Settings.UseDifferentSettingsWhenMaximized;
            StartWithWindowsCheckBox.IsChecked = TaskBarOptions.Options.Settings.StartWhenLaunched;

            try
            {
                fileMedia = new Uri(TaskBarOptions.Options.WallpapperPath);
            }
            catch { }            
        }

        private void SaveSettings() // Метод для сохранения настроек
        {
            TaskBarOptions.Options.WallpapperPath = fileMedia?.ToString();
            TaskBarOptions.Options.Settings.ChooseAFitComboBoxIndex = (byte)WallpaperStretchTypeComboBox.SelectedIndex;
            TaskBarOptions.Options.Settings.MainTaskbarStyle.AccentState = (byte)((int)AccentComboBox.SelectedItem);
            TaskBarOptions.Options.Settings.MainTaskbarStyle.GradientColor = ColorPicker.SelectedColor.ToString();
            TaskBarOptions.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha = (byte)AccentAlphaSlider.Value;
            TaskBarOptions.Options.Settings.MainTaskbarStyle.Colorize = ColorizeCB.IsChecked ?? false;
            TaskBarOptions.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor = WindowsAccentColorCheckBox.IsChecked ?? false;

            TaskBarOptions.Options.Settings.SetWallpapperWhenLaunched = SetWallpapperWhenLaunchedCheckBox.IsChecked ?? false;
            TaskBarOptions.Options.Settings.UseDifferentSettingsWhenMaximized = UseMaximizedSettingsCheckBox.IsChecked ?? false;
            TaskBarOptions.Options.Settings.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;
            TaskBarOptions.Options.Settings.StartWhenLaunched = StartWhenLaunchedCheckBox.IsChecked ?? false;

            TaskBarOptions.SaveOptions();
        }

        private void Window_ContentRendered(object sender, EventArgs e) //7  Общая цель этого метода - инициализировать ваше окно и выполнить необходимые действия после его отображения, включая загрузку настроек, установку начального состояния окна и настройку прослушивания событий.
        {
            // Заполнение ComboBox, загрузка настроек и установка начального состояния окна
            PopulateComboBoxes();
            LoadSettings();
            WindowInitialized = true;

            if (TaskBarOptions.Options.Settings.StartMinimized) { this.WindowState = WindowState.Minimized; }
            if (TaskBarOptions.Options.Settings.StartWhenLaunched) { StartStopButton_Click(null, null); }
            if (TaskBarOptions.Options.Settings.SetWallpapperWhenLaunched) { SetWallpaperButton_Click(null, null); SetWallpaperButton.IsEnabled = true; }

            WallpaperStretchTypeComboBox.SelectedIndex = TaskBarOptions.Options.Settings.ChooseAFitComboBoxIndex;

            // Listen for name change changes across all processes/threads on current desktop
            // Установка хука для отслеживания изменения состояния окна
            WindowStateHook = User32.SetWinEventHook(User32.EventConstants.EVENT_MIN, User32.EventConstants.EVENT_MAX, IntPtr.Zero, procDelegate, 0, 0, User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
        }
        #endregion Initializations

        #region Destructors
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //SysTrayIcon.Dispose();    // Освобождение ресурсов, связанных с иконкой системного трея
            ni.Dispose();
            SaveSettings();
            RunApplyTask = false; // Остановка задачи ApplyTask
            User32.UnhookWinEvent(WindowStateHook); // Отключение хука для отслеживания изменения состояния окна

            //ni.Visible = false;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }           
        #endregion Destructors

        #region Functions
        private void ApplyToAllTaskbars()
        {
            Taskbars.Bars = new List<Taskbar>();

            while (RunApplyTask) // Бесконечный цикл, выполняющийся в отдельном потоке, пока RunApplyTask равно true
            {
                if (FindTaskbarHandles) // Если нужно искать дескрипторы панелей задач
                {

                    Taskbars.Bars.Add(new Taskbar(User32.FindWindow("Shell_TrayWnd", null))); // Добавление главной панели задач в список                    
                    HWND otherBars = IntPtr.Zero;

                    // Поиск и добавление других панелей задач
                    while (true)
                    {
                        otherBars = User32.FindWindowEx(HWND.NULL, otherBars, "Shell_SecondaryTrayWnd", "");
                        if (otherBars == IntPtr.Zero) { break; }
                        else { Taskbars.Bars.Add(new Taskbar(otherBars)); }
                    }

                    FindTaskbarHandles = false; // Завершение поиска дескрипторов панелей задач

                    App.Current.Dispatcher.Invoke(() => UpdateAllSettings()); // Обновление всех настроек для добавленных панелей задач
                }

                if (Taskbars.MaximizedStateChanged) // Если изменилось состояние максимизации окон
                {
                    Taskbars.UpdateMaximizedState();
                    Taskbars.UpdateAllSettings();
                }

                foreach (Taskbar taskbar in Taskbars.Bars) // Применение стилей для каждой панели задачи
                {
                    Taskbars.ApplyStyles(taskbar);
                }

                Thread.Sleep(10); // Задержка на 10 миллисекунд
            }
        }

        protected override void OnSourceInitialized(EventArgs e) //5
        {
            base.OnSourceInitialized(e);

            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle; //Этот код создает переменную mainWindowPtr, которая будет содержать дескриптор окна (Window Handle) вашего WPF окна (this). Дескриптор окна - это числовое значение, которое уникально идентифицирует окно в операционной системе Windows.
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr); //Общий сценарий использования HwndSource заключается в интеграции элементов WPF в существующий Win32 или WinAPI код, где HwndSource позволяет вам связать оконный дескриптор и ресурсы WPF, обеспечивая совместимость между двумя подходами в построении графических интерфейсов.
            mainWindowSrc.AddHook(WndProc); // В вашем коде mainWindowSrc.AddHook(WndProc); вы регистрируете метод WndProc в качестве обработчика оконных сообщений для окна, на которое ссылается mainWindowSrc. Это позволяет вашему WPF окну получать и обрабатывать низкоуровневые оконные сообщения, которые в противном случае могли бы быть обработаны стандартным оконной процедурой WinAPI.
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) //6 // Обработчик сообщений окна //5161151515115
        {
            if (msg == WM_TASKBARCREATED) // Если получено сообщение о создании панели задач
            {
                FindTaskbarHandles = true; // Установка флага для поиска дескрипторов панелей задач
                handled = true;
            }
            else if (msg == (int)User32.WindowMessage.WM_DWMCOLORIZATIONCOLORCHANGED) // Если получено сообщение об изменении цвета акцента Windows
            {
                Globals.WindowsAccentColor = WindowsAccentColor.GetColorAsInt(); // TODO: use colour from wParam 
                                                                                 // Обновление цвета акцента
                handled = true;
            }

            return IntPtr.Zero;
        }

        static void WinEventProc(User32.HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime) //В целом, этот метод служит для отслеживания и получения информации о параметрах окна, связанных с событием, которое вызвало этот обработчик событий. Полученная информация может быть использована для дальнейшей обработки события в вашем приложении. //ничё не понял очень долгий цикл, но скорее всего это тот хук и он постоянно работает
        {  // не надо по моим ощущениям (просто проверяет все окна по их состояниям )           

            User32.WINDOWPLACEMENT placement = new User32.WINDOWPLACEMENT();
            placement.length = (uint)Marshal.SizeOf(placement);
            User32.GetWindowPlacement(hwnd, ref placement);

            // Window is closing
            // Если окно закрывается
            if (idObject == -2 && idChild == 5)
            {
                if (MaximizedWindows.Contains(hwnd)) // Если окно было максимизировано
                {
                    LastClosedWindow = hwnd;
                    LastClosedWindowTime = DateTime.Now;
                    MaximizedWindows.Remove(hwnd);
                    Taskbars.MaximizedStateChanged = true; // Установка флага изменения состояния максимизации
                }
            }
            // Если окно максимизировано
            else if (placement.showCmd == ShowWindowCommand.SW_MAXIMIZE || placement.showCmd == ShowWindowCommand.SW_SHOWMAXIMIZED) //В общем, этот код служит для отслеживания состояния максимизации окон и выполнения действий в зависимости от этого состояния, включая запись информации о максимизированных окнах и возможное изменение состояния максимизации.
            {
                if (!MaximizedWindows.Contains(hwnd)) // Если окно не было добавлено в список максимизированных окон
                {
                    if (LastClosedWindow == hwnd && ((TimeSpan)(DateTime.Now - LastClosedWindowTime)).TotalSeconds < 1) { return; }

                    MaximizedWindows.Add(hwnd);
                    Taskbars.MaximizedStateChanged = true;
                }
            }
            else if (placement.showCmd == ShowWindowCommand.SW_NORMAL) // Если окно в обычном состоянии
            {
                if (MaximizedWindows.Contains(hwnd)) // Если окно было максимизировано
                {
                    MaximizedWindows.Remove(hwnd);
                    Taskbars.MaximizedStateChanged = true;
                }
            }
            else if (placement.showCmd == ShowWindowCommand.SW_SHOWMINIMIZED || placement.showCmd == ShowWindowCommand.SW_MINIMIZE) // Если окно свернуто
            {
                if (MaximizedWindows.Contains(hwnd)) // Если окно было максимизировано
                {
                    MaximizedWindows.Remove(hwnd);
                    Taskbars.MaximizedStateChanged = true;
                }
            }
        }

        private void UpdateAllSettings() // Обновление всех настроек, включая акцент, цвет, флаги и пр.
        {
            SetAccentState((AccentState)AccentComboBox.SelectedItem);
            SetTaskbarColor(ColorPicker.SelectedColor ?? System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            SetAccentFlags(ColorizeCB.IsChecked ?? false);
            WindowsAccentColorCheckBox_Changed(null, null);
            SetWindowsAccentAlpha((byte)AccentAlphaSlider.Value);

            Taskbars.UpdateAllSettings(); // Обновление настроек для всех панелей задач
        }

        private void SwitchTaskbarBeingEdited(string switchTo) //Этот метод служит для изменения контекста редактирования панели задачи в вашем приложении и обновления элементов управления, связанных с этим контекстом.
        {
            TaskbarBeingEdited = switchTo;  // Изменение контекста редактирования
            ShowTaskbarSettings(TaskbarBeingEdited); // Отображение настроек выбранной панели задачи

            if (TaskbarBeingEdited == "Main")
            {
                EditSwitchButton.Content = $"Основная панель задач";
            }
            else if (TaskbarBeingEdited == "Maximized")
            {
                EditSwitchButton.Content = $"Дополнительная панель задач";
            }
            
        }

        private void ShowTaskbarSettings(string tb) //Этот метод позволяет отобразить настройки главной панели задачи в интерфейсе вашего приложения, чтобы пользователь мог видеть текущие значения настроек и, возможно, их изменить.
        {
            // Отображение настроек выбранной панели задачи в интерфейсе приложения
            if (tb == "Main")
            {
                AccentComboBox.SelectedItem = (AccentState)TaskBarOptions.Options.Settings.MainTaskbarStyle.AccentState;
                ColorPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TaskBarOptions.Options.Settings.MainTaskbarStyle.GradientColor);
                AccentAlphaSlider.Value = TaskBarOptions.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha;
                ColorizeCB.IsChecked = TaskBarOptions.Options.Settings.MainTaskbarStyle.Colorize;
                WindowsAccentColorCheckBox.IsChecked = TaskBarOptions.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor;
            }
            else if (tb == "Maximized")
            {
                AccentComboBox.SelectedItem = (AccentState)TaskBarOptions.Options.Settings.MaximizedTaskbarStyle.AccentState;
                ColorPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TaskBarOptions.Options.Settings.MaximizedTaskbarStyle.GradientColor);
                AccentAlphaSlider.Value = TaskBarOptions.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha;
                ColorizeCB.IsChecked = TaskBarOptions.Options.Settings.MaximizedTaskbarStyle.Colorize;
                WindowsAccentColorCheckBox.IsChecked = TaskBarOptions.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor;
            }
        }

        #endregion Functions

        #region Control Handles
        private void StartStopButton_Click(object sender, RoutedEventArgs e) // Обработчик события нажатия кнопки Start/Stop
        {
            if (RunApplyTask)
            {
                StartStopButton.Content = "Начать";
                RunApplyTask = false;
            }
            else
            {
                StartStopButton.Content = "Остановить";
                RunApplyTask = true;

                ApplyTask = new Task(() => ApplyToAllTaskbars()); // Создание и запуск задачи ApplyToAllTaskbars
                ApplyTask.Start();
            }
        }

        private void FakeAccentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Общая цель этого обработчика события - реагировать на изменение выбранного элемента в AccentStateComboBox и выполнить какие-либо действия на основе этого изменения, возможно, изменить параметры приложения связанные с AccentState.
        {   // Обработчик события изменения выбранного элемента в AccentStateComboBox
            if (!WindowInitialized) return;
            AccentComboBox.SelectedIndex = FakeAccentComboBox.SelectedIndex;
            SetAccentState((AccentState)AccentComboBox.SelectedItem);
        }

        private void AccentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            FakeAccentComboBox.SelectedIndex = AccentComboBox.SelectedIndex;
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {   // Обработчик события изменения выбранного цвета в GradientColorPicker
            if (!WindowInitialized) return;

            SetTaskbarColor(ColorPicker.SelectedColor ?? System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        }

        private void ColorizeCB_Changed(object sender, RoutedEventArgs e)
        {   // Обработчик события изменения состояния флажка ColorizeBlurCheckBox
            if (!WindowInitialized) return;
            SetAccentFlags(ColorizeCB.IsChecked ?? false);
        }

        //private void SysTrayIcon_MouseClick(object sender, EventArgs e)
        //{   // Обработчик события клика мыши на иконке в системном лотке
        //    System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
        //    if (me.Button == System.Windows.Forms.MouseButtons.Right)
        //    {
        //        SysTrayContextMenu.PlacementTarget = sender as Button;
        //        SysTrayContextMenu.IsOpen = true;
        //        this.Activate();
        //    }
        //}

        private void WindowsAccentColorCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Globals.WindowsAccentColor = WindowsAccentColor.GetColorAsInt();

            if (!WindowInitialized) return;

            bool use = WindowsAccentColorCheckBox.IsChecked ?? false; //4564165456465468546584646864846846894896984498
            SetUseAccentColor(use);
            //GradientColorPicker.IsEnabled = !use;
        }

        private void UseMaximizedSettingsCheckBox_Changed(object sender, RoutedEventArgs e)
        {   // Обработчик события изменения состояния флажка UseMaximizedSettingsCheckBox
            if (!WindowInitialized) return;
            TaskBarOptions.Options.Settings.UseDifferentSettingsWhenMaximized = UseMaximizedSettingsCheckBox.IsChecked ?? false;
            Taskbars.UpdateAllSettings();
        }

        private void AccentAlphaSlider_DragCompleted(object sender, RoutedEventArgs e)
        {   // Обработчик события завершения перетаскивания ползунка WindowsAccentAlphaSlider
            alphaDragStarted = false;
            SetWindowsAccentAlpha((byte)AccentAlphaSlider.Value);
        }

        private void AccentAlphaSlider_DragStarted(object sender, RoutedEventArgs e)
        {   // Обработчик события начала перетаскивания ползунка WindowsAccentAlphaSlider
            alphaDragStarted = true;
        }

        private void AccentAlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // по факту нахуй не надо, но я думаю добавить поле где человек может вручную написать проценты а не ползунком 
        {   // Обработчик события изменения значения ползунка WindowsAccentAlphaSlider
            if (!WindowInitialized) return;
            if (!alphaDragStarted) // Если убрать ! то будет в режиме реального времени (но хз мешает чемуто или памяти мнгого или тд хз почему так сделал чел)  // проверял там аль размытие лагало вернул "!"
            {
                //SetWindowsAccentAlpha((byte)AccentAlphaSlider.Value);
            }
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!WindowInitialized) return;

            TaskBarOptions.Options.Settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

            try
            {
                if (TaskBarOptions.Options.Settings.StartWithWindows) { rkApp.SetValue("WiPapper", $"\"{MyPath}\""); }
                else { rkApp.DeleteValue("WiPapper", false); }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Failed to Set Registry Key");  //исправить на русский
            }
        }

        private void EditSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskbarBeingEdited == "Main")
            {
                SwitchTaskbarBeingEdited("Maximized");
            }
            else if (TaskbarBeingEdited == "Maximized")
            {
                SwitchTaskbarBeingEdited("Main");
            }
        }
        #endregion Control Handles


    }
}
