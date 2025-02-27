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
        public static List<HWND> MaximizedWindows = new List<HWND>();
        public static List<Taskbar> HwndMonitors = new List<Taskbar>();
        public static string TaskbarBeingEdited = "Main";
        public static int WindowsAccentColor;

        private static Int32 ColorToInt32(string color, string taskbar)
        {
            Color thisColor = (Color)ColorConverter.ConvertFromString(color);
            return ColorToInt32(thisColor, taskbar);
        }

        private static Int32 ColorToInt32(Color color, string taskbar)
        {
            if (taskbar == "Main")
            {
                return (Int32)BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, OptionsManager.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha }, 0);
            }
            else
            {
                return (Int32)BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, OptionsManager.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha }, 0);
            }
        }

        #region TaskbarColor
        public static Int32 GetTaskbarColor(string taskbar)
        {
            if (taskbar == "Main")
            {
                if (OptionsManager.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor)
                {
                    byte[] bytes = BitConverter.GetBytes(WindowsAccentColor);
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], OptionsManager.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha }, 0);
                    return colorInt;
                }
                else
                {
                    return ColorToInt32(OptionsManager.Options.Settings.MainTaskbarStyle.GradientColor, taskbar);
                }
            }
            else
            {
                if (OptionsManager.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor)
                {
                    byte[] bytes = BitConverter.GetBytes(WindowsAccentColor);
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], OptionsManager.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha }, 0);
                    return colorInt;
                }
                else { return ColorToInt32(OptionsManager.Options.Settings.MaximizedTaskbarStyle.GradientColor, taskbar); }
            }
        }

        public static void SetTaskbarColor(Color color)
        {
            if (TaskbarBeingEdited == "Main")
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
        public static int GetAccentFlags(string taskbar)
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

        public static void SetAccentFlags(bool colorize)
        {
            if (TaskbarBeingEdited == "Main")
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
        public static AccentState GetAccentState(string taskbar)
        {
            if (taskbar == "Main")
            {
                return (AccentState)OptionsManager.Options.Settings.MainTaskbarStyle.AccentState;
            }
            else
            {
                return (AccentState)OptionsManager.Options.Settings.MaximizedTaskbarStyle.AccentState;
            }
        }

        public static void SetAccentState(int stateIndex)
        {
            if (TaskbarBeingEdited == "Main")
            {
                OptionsManager.Options.Settings.MainTaskbarStyle.AccentState = (byte)stateIndex; 
            }
            else
            {
                OptionsManager.Options.Settings.MaximizedTaskbarStyle.AccentState = (byte)stateIndex;
            }
        }
        #endregion AccentState

        #region UseAccentColor
        public static void SetUseAccentColor(bool use)
        {
            if (TaskbarBeingEdited == "Main")
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
        public static void SetWindowsAccentAlpha(byte alpha)
        {
            if (TaskbarBeingEdited == "Main")
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
