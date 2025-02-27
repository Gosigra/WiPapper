using Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using WiPapper.AppOptions;

namespace WiPapper
{
    public class Taskbar
    {
        public HWND HWND { get; set; } //Дескриптор окна панели задачи.
        public HMONITOR Monitor { get; set; } //Свойство Monitor типа IntPtr, представляющее дескриптор монитора панели задачи. 
                                              //Монитор панели задачи указывает на то, к какому конкретному монитору относится определенная панель задачи.
        public bool HasMaximizedWindow { get; set; }
        public AccentPolicy AccentPolicy; //Стили акцентирования для панели задачи.

        public Taskbar(HWND hwnd)
        {
            HWND = hwnd;
            Monitor = User32.MonitorFromWindow(hwnd, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST); //Определение монитора для панели задачи.
            AccentPolicy = new AccentPolicy();

            FindMaximizedWindowsHere();
        }

        public void FindMaximizedWindowsHere()
        {
            bool isInThisScreen = false;
            IntPtr thisAppMonitor;

            foreach (IntPtr hwnd in Globals.MaximizedWindows)
            {
                thisAppMonitor = (IntPtr)User32.MonitorFromWindow(hwnd, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST); //Определение монитора для текущего окна.
                if (Monitor == thisAppMonitor) { isInThisScreen = true; } //Проверка, находится ли окно на том же мониторе, что и панель задачи.
            }

            HasMaximizedWindow = isInThisScreen;
        }
    }

    public static class Taskbars
    {
        public static List<Taskbar> Bars { get; set; }
        public static bool MaximizedStateChanged { get; set; }
        private static string tbType;

        static Taskbars()
        {
            Bars = new List<Taskbar>();
            MaximizedStateChanged = true;
        }

        public static void ApplyStyles(Taskbar taskbar)
        {
            int sizeOfPolicy = Marshal.SizeOf(taskbar.AccentPolicy); //Определение размера структуры AccentPolicy в байтах.
            IntPtr policyPtr = Marshal.AllocHGlobal(sizeOfPolicy);   //Выделение памяти для структуры AccentPolicy.
            Marshal.StructureToPtr(taskbar.AccentPolicy, policyPtr, false); //Копирование данных из объекта taskbar.AccentPolicy в выделенную память.

            WindowCompositionAttribData data = new WindowCompositionAttribData(WindowsCompositionAttribute.WCA_ACCENT_POLICY, policyPtr, sizeOfPolicy); //Создание объекта WinCompatTrData с данными для установки атрибутов композиции окна.

            SWCA.SetWindowCompositionAttribute(taskbar.HWND, ref data);

            Marshal.FreeHGlobal(policyPtr); //Освобождение выделенной памяти.
        }

        public static void UpdateMaximizedState()
        {
            foreach (Taskbar tb in Bars)
            {
                tb.FindMaximizedWindowsHere();
            }
            MaximizedStateChanged = false;
        }

        public static void UpdateAllSettings()
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType);
                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
                tb.AccentPolicy.GradientColor = Globals.GetTaskbarColor(tbType);
            }
        }

        public static void UpdateAccentState()
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType);
            }
        }

        public static void UpdateAccentFlags()
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
            }
        }

        public static void UpdateColor()
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.GradientColor = Globals.GetTaskbarColor(tbType);
            }
        }
    }
}