﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Reflection;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using Microsoft.Win32;
using static WiPapper.Globals;
using WiPapper.Wallpaper.HtmlWallpaper;
using WiPapper.AppOptions;
using System.Diagnostics;
using WiPapper.DB;
using System.Text.RegularExpressions;


namespace WiPapper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Реестровый ключ для запуска с Windows
        readonly RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true); //Registry.CurrentUser = HKEY_CURRENT_USER
        NotifyIcon notifyIcon;

        DataBase dataBase = new DataBase();
        

        #region wlp
        public static List<Rectangle> ScreenList;
        public static List<Window> WindowList;
        public static List<MediaElement> MediaList;

        public static HWND workerw;

        public static Uri WallpaperPath;

        public static bool currentlySet = false;
        #endregion

        #region Declarations TB
        bool WindowInitialized = false;  // Флаг инициализации окна
        readonly string MyPath = Assembly.GetExecutingAssembly().Location;       // Путь к исполняемому файлу

        // Объявление задачи ApplyTask и флага RunApplyTask
        static Task ApplyTask;
        static bool RunApplyTask = false;
        public static bool FindTaskbarHandles = true;

        private static bool alphaDragStarted = false;

        // Коды сообщений для перезапуска проводника и изменения цвета акцента Windows
        private static readonly uint WM_TASKBARCREATED = User32.RegisterWindowMessage("TaskbarCreated");

        // Хук для отслеживания изменения состояния окна
        private static readonly User32.WinEventProc procDelegate = new User32.WinEventProc(WinEventProc);
        private static User32.HWINEVENTHOOK WindowStateHook;
        private static HWND LastClosedWindow;
        private static DateTime LastClosedWindowTime;
        #endregion Declarations

        public MainWindow()
        {
            InitializeComponent();

            InitializeNotifyIcon();

            SetHtmlWallpaper.StartHttpListener();

            dataBase.Start();

            DataContext = this;
            FillWallpapersContainer();

            FillLists();
        }

        private async void FillWallpapersContainer()
        {
            WallpapersContainer.ItemsSource = await dataBase.LoadImageDetailsFromSupabase();
        }

        private void FillLists()
        {
            ScreenList = new List<Rectangle>();
            WindowList = new List<Window>();
            MediaList = new List<MediaElement>();
            foreach (var item in Screen.AllScreens)
            {
                ScreenList.Add(item.Bounds);                                                                                                 //получаю рабочий стол и его рабочую область ({X = 0 Y = 0 Width = 1920 Height = 1040}) //working area
            }
            for (int i = 0; i < ScreenList.Count; i++)
            {
                WindowList.Add(new Window());                                                                                                                //здесь кол-во экранов
                MediaList.Add(new MediaElement());
            }
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();                                                                                                     // Инициализация иконки системного трея
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("Resources/1.ico", UriKind.Relative)).Stream;
            notifyIcon.Icon = new Icon(iconStream);
            notifyIcon.Click += (object sender, EventArgs args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
        }

        protected override void OnStateChanged(EventArgs e) // Уведомление о сворачивании
        {
            if(WindowState == WindowState.Minimized)
            {
                notifyIcon.BalloonTipTitle = "WiPapper";
                notifyIcon.BalloonTipText = "WiPapper было свёрнуто";
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(10000);
                //ni.ShowBalloonTip(10000, "WiPaper", "WiPaper было свёрнуто", ToolTipIcon.Info);
                Hide();
            }
            else if (WindowState.Normal == this.WindowState)
            {
                notifyIcon.Visible = false;
            }
            base.OnStateChanged(e);
        }

        private void LoadSettings()                                                                                                 //Этот метод позволяет загрузить сохраненные настройки и отразить их в интерфейсе вашего приложения, чтобы пользователь мог видеть текущие значения настроек.
        {
            OptionsManager.InitializeOptions();

            SwitchTaskbarBeingEdited("Main");

            SetWallpapperWhenLaunchedCheckBox.IsChecked = OptionsManager.Options.SetWallpapperWhenLaunched;
            StartMinimizedCheckBox.IsChecked = OptionsManager.Options.StartMinimized;
            StartWhenLaunchedCheckBox.IsChecked = OptionsManager.Options.StartWhenLaunched;
            UseMaximizedSettingsCheckBox.IsChecked = OptionsManager.Options.UseDifferentSettingsWhenMaximized;
            StartWithWindowsCheckBox.IsChecked = OptionsManager.Options.StartWithWindows;
            DefaultInstallationPath.Text = OptionsManager.Options.Settings.DefaultInstallationPath;

            try
            {
                WallpaperPath = new Uri(OptionsManager.Options.Settings.WallpapperPath);
            }
            catch{} //что то сделать
        }

        private void SaveSettings()                                                                                                                 // Метод для сохранения настроек  //тут сохраняются настройки приложения поэтому надо перенести место сохранения (сохранять не в TaskBarOptions) или переименовать TaskBarOptions тк там все сохраняется или хуй знает
        {
            OptionsManager.Options.Settings.DefaultInstallationPath = DefaultInstallationPath.Text ?? string.Empty;
            OptionsManager.Options.Settings.WallpapperPath = WallpaperPath?.ToString();
            OptionsManager.Options.ChooseAFitComboBoxIndex = (byte)WallpaperStretchTypeComboBox.SelectedIndex;

            OptionsManager.Options.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;
            OptionsManager.Options.SetWallpapperWhenLaunched = SetWallpapperWhenLaunchedCheckBox.IsChecked ?? false;
            OptionsManager.Options.StartWhenLaunched = StartWhenLaunchedCheckBox.IsChecked ?? false;

            OptionsManager.SerializeOptions();
        }

        private void Window_ContentRendered(object sender, EventArgs e) //7  Общая цель этого метода - инициализировать ваше окно и выполнить необходимые действия после его отображения, включая загрузку настроек, установку начального состояния окна и настройку прослушивания событий.
        {
            LoadSettings();
            WindowInitialized = true;

            if (OptionsManager.Options.StartMinimized)
            {
                this.WindowState = WindowState.Minimized;
            }

            if (OptionsManager.Options.StartWhenLaunched)
            {
                StartStopButton_Click(null, null);
            }

            if (OptionsManager.Options.SetWallpapperWhenLaunched)
            {
                SetWallpaperButton.IsEnabled = true;
                SetWallpaperButton_Click(null, null);
            }

            WallpaperStretchTypeComboBox.SelectedIndex = OptionsManager.Options.ChooseAFitComboBoxIndex;

            // Установка хука для отслеживания изменения состояния окна
            WindowStateHook = User32.SetWinEventHook(User32.EventConstants.EVENT_MIN, User32.EventConstants.EVENT_MAX, IntPtr.Zero, procDelegate, 0, 0, User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
        }

        private void UseMaximizedSettingsCheckBox_Changed(object sender, RoutedEventArgs e) 
        {
            if (!WindowInitialized) return;
            OptionsManager.Options.UseDifferentSettingsWhenMaximized = UseMaximizedSettingsCheckBox.IsChecked ?? false;
            Taskbars.UpdateAllSettings();
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!WindowInitialized) return;

            OptionsManager.Options.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

            try
            {
                if (OptionsManager.Options.StartWithWindows) { rkApp.SetValue("WiPapper", $"\"{MyPath}\""); }
                else { rkApp.DeleteValue("WiPapper", false); }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Не удалось установить ключ реестра");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            notifyIcon.Visible = false;

            if (MediaList != null)
            {
                foreach (Window window in WindowList)
                {
                    window.Close();
                }
            }

            base.OnClosing(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Освобождение ресурсов, связанных с иконкой системного трея
            notifyIcon.Dispose();
            SaveSettings();
            RunApplyTask = false; // Остановка задачи ApplyTask
            User32.UnhookWinEvent(WindowStateHook); // Отключение хука для отслеживания изменения состояния окна
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #region Wallpaper
        private void SetMediaAsWallpaper(Window windowList, int i)
        {
            MediaList[i] = new MediaElement();
            Grid grid = new Grid();
            windowList.Content = grid;
            grid.Children.Add(MediaList[i]);

            MediaList[i].Source = WallpaperPath;
            MediaList[i].LoadedBehavior = MediaState.Manual;
            MediaList[i].Volume = 0;

            MediaList[i].MediaEnded += (send, eArgs) =>
            {
                MediaList[i].Position = new TimeSpan(0, 0, 1);
                MediaList[i].Play();
            };
            MediaList[i].Play();
            currentlySet = true;
        }

        private void UnSetWall()
        {
            if (currentlySet)
            {
                MediaSessionHandler.oldThumbnailUrl = string.Empty;
                AudioProcessor.Capture.StopRecording();
                foreach (Window window in WindowList)
                {
                    window.Close();
                }

                currentlySet = false;
            }
        }

        public static void FindWorkerWindow()
        {
            HWND progMan = User32.FindWindow("ProgMan", null);
            IntPtr result = IntPtr.Zero;
            User32.SendMessageTimeout(progMan, 0x052C, new IntPtr(0), IntPtr.Zero, User32.SMTO.SMTO_NORMAL, 1000, ref result);
            workerw = IntPtr.Zero;
            User32.EnumWindows(new User32.EnumWindowsProc((tophandle, intPtr) =>
            {
                HWND p = User32.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (p != IntPtr.Zero)
                {
                    workerw = User32.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                }
                return true;
            }), IntPtr.Zero);
        }

        #region Events
        private void SelectWall_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "All files(*.*)|*.*";

            var rezult = fileDialog.ShowDialog();

            switch (rezult)
            {
                case System.Windows.Forms.DialogResult.OK:
                    //file = fileDialog.FileName;
                    WallpaperPath = new Uri(fileDialog.FileName);
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
            if (WallpaperPath == null)            
            {
                SetWallpaperButton.IsEnabled = false;
                return;
            }
            if (currentlySet)
            {
                UnSetWall();
            }
            InitializeWindows();
        }

        private void InitializeWindows()
        {
            for (int i = 0; i < WindowList.Count; i++)
            {
                WindowList[i] = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,

                    Top = ScreenList[i].Top,
                    Left = ScreenList[i].Left,
                    Width = ScreenList[i].Width,
                    Height = ScreenList[i].Height
                };
                WindowList[i].Initialized += Window_Initialized;
                WindowList[i].UpdateLayout();
                WindowList[i].Show();
                WallpaperStretchTypeComboBox_SelectionChanged(null, null);
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            int i = WindowList.IndexOf(window);
            if (WallpaperPath.AbsolutePath.Contains("index.html"))
            {
                SetHtmlWallpaper.FilePath = Path.GetDirectoryName(WallpaperPath.LocalPath);
                SetHtmlWallpaper.SetBrowserAsWallpaper(window);
            }
            else
            {
                SetMediaAsWallpaper(window, i);
            }
            currentlySet = true;
            HWND windowHandle = new WindowInteropHelper(window).Handle;
            User32.SetParent(windowHandle, workerw);
        }

        private void UnSetWallpaper_Click(object sender, RoutedEventArgs e)
        {
            UnSetWall();
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentlySet)
            MediaList[0].Volume = Volume.Value;
        }

        private void WallpaperStretchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!WindowInitialized || MediaList == null) return;

            foreach (MediaElement mediaItem in MediaList)
            {
                mediaItem.Stretch = (Stretch)WallpaperStretchTypeComboBox.SelectedIndex;
            }
        }

        #endregion

        #endregion

        #region TaskBar
        private void ApplyToAllTaskbars() // панель задач одна так что хз проверить с 2 мониками
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

                    App.Current.Dispatcher.Invoke(() => UpdateAllTBSettings()); // Обновление всех настроек для добавленных панелей задач
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

        protected override void OnSourceInitialized(EventArgs e) //5 //переопределяет базовый метод OnSourceInitialized в классе Window.Этот метод вызывается, когда инициализируется источник окна, то есть когда окно было создано и готово к отображению.
        {
            base.OnSourceInitialized(e); //В контексте WPF, класс Window имеет событие SourceInitialized, которое срабатывает, когда источник окна был инициализирован.
                                         //Если вы переопределяете метод OnSourceInitialized в производном классе, вызов base.OnSourceInitialized(e) гарантирует, что все обработчики событий SourceInitialized в базовом классе Window также будут вызваны.
                                         //Это важно, потому что базовый класс может иметь важную логику, связанную с этим событием, которую нужно выполнить.
                                         //Если вы не вызовете base.OnSourceInitialized(e), эта логика будет пропущена, что может привести к непредвиденным последствиям.

            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle; //Этот код создает переменную mainWindowPtr, которая будет содержать дескриптор окна (Window Handle) вашего WPF окна (this). Дескриптор окна - это числовое значение, которое уникально идентифицирует окно в операционной системе Windows.
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr); //Общий сценарий использования HwndSource заключается в интеграции элементов WPF в существующий Win32 или WinAPI код, где HwndSource позволяет вам связать оконный дескриптор и ресурсы WPF, обеспечивая совместимость между двумя подходами в построении графических интерфейсов.
            mainWindowSrc.AddHook(WndProc); // В вашем коде mainWindowSrc.AddHook(WndProc); вы регистрируете метод WndProc в качестве обработчика оконных сообщений для окна, на которое ссылается mainWindowSrc. Это позволяет вашему WPF окну получать и обрабатывать низкоуровневые оконные сообщения, которые в противном случае могли бы быть обработаны стандартным оконной процедурой WinAPI.(кароче система отправляет сообщения хук их перехватывает так сказать, а wndproc это обработцик этих сообщений)
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) //6 // Обработчик сообщений окна //5161151515115
        {
            if (msg == WM_TASKBARCREATED) // Если получено сообщение о создании панели задач // это системное сообщение, которое операционная система Windows отправляет всем окнам, когда создается новая панель задач.
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

            //Таким образом, хотя ваше приложение в основном обрабатывает сообщения, связанные с его собственным окном, оно также может обрабатывать определенные системные сообщения, такие как WM_TASKBARCREATED, которые относятся к глобальным событиям в операционной системе. Это позволяет вашему приложению реагировать на изменения в системе, такие как создание новой панели задач.
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

        private void UpdateAllTBSettings() // Обновление всех настроек, включая акцент, цвет, флаги и пр. TaskBar
        {
            SetAccentState(AccentComboBox.SelectedIndex);
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
                EditSwitchTextBlock.Text = "Основная панель задач";
            }
            else if (TaskbarBeingEdited == "Maximized") //additional
            {
                EditSwitchTextBlock.Text = "Дополнительная панель задач";
            }
        }

        private void ShowTaskbarSettings(string tb) //Этот метод позволяет отобразить настройки главной панели задачи в интерфейсе вашего приложения, чтобы пользователь мог видеть текущие значения настроек и, возможно, их изменить.
        {
            // Отображение настроек выбранной панели задачи в интерфейсе приложения
            if (tb == "Main")
            {
                AccentComboBox.SelectedIndex = OptionsManager.Options.Settings.MainTaskbarStyle.AccentState;
                ColorPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(OptionsManager.Options.Settings.MainTaskbarStyle.GradientColor);
                AccentAlphaSlider.Value = OptionsManager.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha;
                ColorizeCB.IsChecked = OptionsManager.Options.Settings.MainTaskbarStyle.Colorize;
                WindowsAccentColorCheckBox.IsChecked = OptionsManager.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor;
            }
            else if (tb == "Maximized")
            {
                AccentComboBox.SelectedIndex = OptionsManager.Options.Settings.MaximizedTaskbarStyle.AccentState;
                ColorPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(OptionsManager.Options.Settings.MaximizedTaskbarStyle.GradientColor);
                AccentAlphaSlider.Value = OptionsManager.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha;
                ColorizeCB.IsChecked = OptionsManager.Options.Settings.MaximizedTaskbarStyle.Colorize;
                WindowsAccentColorCheckBox.IsChecked = OptionsManager.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor;
            }
        }

        #region Events
        private void AccentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Общая цель этого обработчика события - реагировать на изменение выбранного элемента в AccentStateComboBox и выполнить какие-либо действия на основе этого изменения, возможно, изменить параметры приложения связанные с AccentState.
        {   // Обработчик события изменения выбранного элемента в AccentStateComboBox
            if (!WindowInitialized) return;
            SetAccentState(AccentComboBox.SelectedIndex);
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {   // Обработчик события изменения выбранного цвета в GradientColorPicker
            if (!WindowInitialized) return;

            SetTaskbarColor(ColorPicker.SelectedColor ?? System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        }

        private void WindowsAccentColorCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Globals.WindowsAccentColor = WindowsAccentColor.GetColorAsInt();

            if (!WindowInitialized) return;

            bool use = WindowsAccentColorCheckBox.IsChecked ?? false; //4564165456465468546584646864846846894896984498
            SetUseAccentColor(use);
            //GradientColorPicker.IsEnabled = !use;
        }

        private void ColorizeCB_Changed(object sender, RoutedEventArgs e)
        {   // Обработчик события изменения состояния флажка ColorizeBlurCheckBox
            if (!WindowInitialized) return;
            SetAccentFlags(ColorizeCB.IsChecked ?? false);
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
                FindTaskbarHandles = true;

                ApplyTask = new Task(() => ApplyToAllTaskbars()); // Создание и запуск задачи ApplyToAllTaskbars
                ApplyTask.Start();

            }
        }
        #endregion

        #endregion




        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
           bool isAuthorized = await DataBase.Authorize(AutorizationEmailTextBox.Text, AutorizationPasswordTextBox.Password);

            AccountTabControl.Visibility = isAuthorized ? Visibility.Collapsed : Visibility.Visible;
            UserAutorizedPanel.Visibility = isAuthorized ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string name = RegisterNameTextBox.Text;
            string email = RegisterEmailTextBox.Text;
            string password = RegisterPasswordBox.Password;            

            if (name == string.Empty || email == string.Empty || password == string.Empty)
            {
                System.Windows.MessageBox.Show("Заполните все поля", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!IsValidEmail(email))
            {
                System.Windows.MessageBox.Show("Почта введена неверно", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (RegisterPasswordBox.Password.Length < 6)
            {
                System.Windows.MessageBox.Show("Пароль должен состоять минимум из 6 символов", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                bool isAuthorized = await DataBase.CreateAccount(RegisterNameTextBox.Text, RegisterEmailTextBox.Text, RegisterPasswordBox.Password);

                AccountTabControl.Visibility = isAuthorized ? Visibility.Collapsed : Visibility.Visible;
                UserAutorizedPanel.Visibility = isAuthorized ? Visibility.Visible : Visibility.Collapsed;
            }

            RegisterNameTextBox.Text = null;
            RegisterEmailTextBox.Text = null;
            RegisterPasswordBox.Password = null;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Проверка с помощью регулярного выражения
                string pattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                // Если произошла ошибка при проверке, считаем адрес недействительным
                return false;
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            await UploadWallpapers();
            FillWallpapersContainer();
        }

        private async Task UploadWallpapers()
        {
            try
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        UploadProgressGrid.Visibility = Visibility.Visible;
                        string folderPath = folderDialog.SelectedPath;
                        string rootFolderName = Path.GetFileName(folderPath);
                        Guid userId = Guid.Parse(DataBase.session.User.Id);

                        long maxSize = 30 * 1024 * 1024;
                        long folderSize = GetDirectorySize(folderPath);
                        if (maxSize > folderSize)
                        {
                            await DataBase.UploadFolderAsync(folderPath, rootFolderName, userId);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Размер папки превышает максимальный размер и не будет загружен.");
                        }

                        UploadProgressGrid.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                UploadProgressGrid.Visibility = Visibility.Collapsed;
            }
        }

        public static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            long size = 0;
            // Add file sizes.
            FileInfo[] fis = dirInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = dirInfo.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di.FullName);
            }
            return size;
        }

        private void SendEmailButton_Click(object sender, RoutedEventArgs e)
        {
            string name = FeedbackNameTextBox.Text;
            string subject = FeedbackSubjectTextBox.Text;
            string message = FeedbackBodyTextBox.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                System.Windows.MessageBox.Show("Пожалуйста заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Feedback.SendEmail(name, subject, message);
                System.Windows.MessageBox.Show("Ваше сообщение отправлено.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка отправки сообщения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            FeedbackNameTextBox.Text = null;
            FeedbackSubjectTextBox.Text = null;
            FeedbackBodyTextBox.Text = null;
        }

        private void CreateWallpaperButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("TemplateWallpaper"))
            {
                System.Windows.MessageBox.Show("Если у вас возникла данная ошибка это значит что папки с шаблоном обоев нет. Сообщите о своей ошибке через обратную связь или переустановите приложение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Process.Start("explorer.exe", "TemplateWallpaper");
        }
    }
}
