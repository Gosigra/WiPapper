using Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Vanara.PInvoke;
using WiPapper.AppOptions;

namespace WiPapper
{
    public static class Globals
    {
        public static List<HWND> MaximizedWindows = new List<HWND>(); //Объявление статического списка MaximizedWindows, который содержит указатели на максимизированные окна.
        public static List<Taskbar> HwndMonitors = new List<Taskbar>(); //Объявление статического списка HwndMonitors, который содержит объекты Taskbar.
        public static string TaskbarBeingEdited = "Main"; //cодержит идентификатор редактируемой панели задач. //я добавил main
        public static int WindowsAccentColor; // содержащей цвет акцента Windows.
        //public static SettingsClass TaskbarSettings = new SettingsClass();

        private static Int32 ColorToInt32(string color, string taskbar) // Приватный метод для преобразования строкового представления цвета в 32-битное целое число.
        {
            Color thisColor = (Color)ColorConverter.ConvertFromString(color);  // Преобразование строки цвета в объект Color
            return ColorToInt32(thisColor, taskbar);// Возврат Int32 представления цвета
        }

        private static Int32 ColorToInt32(Color color, string taskbar) //Приватный метод для преобразования объекта типа Color в 32-битное целое число.
        {
            if (taskbar == "Main") //проверка уже моя добавил string taskbar в ColorToInt32
            {
                return (Int32)BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, OptionsManager.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha }, 0); //xdrtyxfgfhghjcghj return (Int32)BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, color.A}, 0);
            }
            else
            {
                return (Int32)BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, OptionsManager.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha }, 0);
            }
            //// Возврат Int32 представления цвета
        }

        #region TaskbarColor
        public static Int32 GetTaskbarColor(string taskbar) // Получение цвета панели задач в зависимости от типа ("Main" или другой)
        {
            if (taskbar == "Main")
            {
                if (OptionsManager.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor) // Если используется цвет акцента Windows
                {
                    byte[] bytes = BitConverter.GetBytes(WindowsAccentColor); // Получение байтового представления цвета акцента Windows
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], OptionsManager.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha }, 0); // Преобразование в Int32, включая альфа-канал из настроек панели задач
                    return colorInt;
                }
                else
                {
                    return ColorToInt32(OptionsManager.Options.Settings.MainTaskbarStyle.GradientColor, taskbar); // Возврат Int32 представления цвета из настроек градиента
                }
            }
            else
            {
                if (OptionsManager.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor)  //Аналогично но только с maximized
                {
                    byte[] bytes = BitConverter.GetBytes(WindowsAccentColor);
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], OptionsManager.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha }, 0);
                    return colorInt;
                }
                else { return ColorToInt32(OptionsManager.Options.Settings.MaximizedTaskbarStyle.GradientColor, taskbar); }
            }
        }

        public static void SetTaskbarColor(Color color) // Установка цвета панели задач // разделено на 2 метода для удобства тк если бы он сделал в одномм методе
                                                        // то пришлось бы передавать каждый раз TaskbarBeingEdited при вызове из других частей кода
                                                        // (хоотя хз можно и не передавать TaskbarBeingEdited а просто его написать в проверке)
        {
            SetTaskbarColor(TaskbarBeingEdited, color);
        }

        public static void SetTaskbarColor(string taskbar, Color color) // Установка цвета панели задач для определенного типа
        {
            if (taskbar == "Main") //TaskbarBeingEdited == "Main" вот так
            {
                OptionsManager.Options.Settings.MainTaskbarStyle.GradientColor = color.ToString();
            }
            else
            {
                OptionsManager.Options.Settings.MaximizedTaskbarStyle.GradientColor = color.ToString();
            }
        }
        #endregion TaskbarColor

        #region AccentFlags
        public static int GetAccentFlags(string taskbar) // Получение флагов акцента в зависимости от типа панели задач
        {
            if (taskbar == "Main")
            {
                if (OptionsManager.Options.Settings.MainTaskbarStyle.Colorize) { return 2; }
                else { return 0; }
            }
            else
            {
                if (OptionsManager.Options.Settings.MaximizedTaskbarStyle.Colorize) { return 2; }
                else { return 0; }
            }
        }

        public static void SetAccentFlags(bool colorize) //аналогично
        {
            SetAccentFlags(TaskbarBeingEdited, colorize);
        }

        public static void SetAccentFlags(string taskbar, bool colorize) //аналогично
        {
            if (taskbar == "Main")
            {
                OptionsManager.Options.Settings.MainTaskbarStyle.Colorize = colorize;
            }
            else
            {
                OptionsManager.Options.Settings.MaximizedTaskbarStyle.Colorize = colorize;
            }
        }
        #endregion AccentFlags

        #region AccentState
        public static AccentState GetAccentState(string taskbar) // Получение состояния акцента для определенной панели задач
        {
            if (taskbar == "Main")
            {
                return (AccentState)OptionsManager.Options.Settings.MainTaskbarStyle.AccentState; // Возврат состояния акцента из настроек главной панели задач
            }
            else
            {
                return (AccentState)OptionsManager.Options.Settings.MaximizedTaskbarStyle.AccentState; // Возврат состояния акцента из настроек максимизированной панели задач
            }
        }

        public static void SetAccentState(int stateIndex) //аналогично //принимать инт
        {
            SetAccentState(TaskbarBeingEdited, stateIndex);
        }
        //Эти методы позволяют управлять и изменять состояние акцента для различных панелей задачи в зависимости от текущего контекста редактирования в вашем приложении.

        public static void SetAccentState(string taskbar, int stateIndex) //аналогично
        {
            if (taskbar == "Main")
            {
                OptionsManager.Options.Settings.MainTaskbarStyle.AccentState = (byte)stateIndex; // ну и тут соответственно просто инт, а то сначала число потом перевод в название а тут опять перевод в число
            }
            else
            {
                OptionsManager.Options.Settings.MaximizedTaskbarStyle.AccentState = (byte)stateIndex;
            }
        }
        #endregion AccentState

        #region UseAccentColor
        public static void SetUseAccentColor(bool use) //аналогично
        {
            SetUseAccentColor(TaskbarBeingEdited, use);
        }

        public static void SetUseAccentColor(string taskbar, bool use) //аналогично
        {
            if (taskbar == "Main")
            {
                OptionsManager.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor = use;
            }
            else
            {
                OptionsManager.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor = use;
            }
        }
        #endregion UseAccentColor

        #region WindowsAccentAlpha
        public static void SetWindowsAccentAlpha(byte alpha) // Установка уровня прозрачности цвета акцента для текущей редактируемой панели задач
        {
            SetWindowsAccentAlpha(TaskbarBeingEdited, alpha);
        }

        public static void SetWindowsAccentAlpha(string taskbar, byte alpha) //аналогично
        {
            if (taskbar == "Main")
            {
                OptionsManager.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha = alpha;
            }
            else
            {
                OptionsManager.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha = alpha;
            }
        }
        #endregion WindowsAccentAlpha
    }
}
