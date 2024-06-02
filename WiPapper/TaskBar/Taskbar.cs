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
        public HWND HWND { get; set; } //Свойство HWND типа IntPtr, представляющее дескриптор окна панели задачи.
        public HMONITOR Monitor { get; set; } //Свойство Monitor типа IntPtr, представляющее дескриптор монитора панели задачи. 
                                            //Монитор панели задачи указывает на то, к какому конкретному монитору относится определенная панель задачи.
        public bool HasMaximizedWindow { get; set; } //Свойство HasMaximizedWindow типа bool, указывающее, содержит ли панель задачи максимизированные окна.
        public AccentPolicy AccentPolicy; // представляющее стили акцентирования для панели задачи.

        public Taskbar(HWND hwnd) // Публичный конструктор класса Taskbar, принимающий дескриптор окна панели задачи.
        {
            HWND = hwnd;
            Monitor = User32.MonitorFromWindow(hwnd, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST); //Определение монитора для панели задачи.
            AccentPolicy = new AccentPolicy();

            FindMaximizedWindowsHere(); //Вызов метода для поиска максимизированных окон.
        }

        public void FindMaximizedWindowsHere()
        {
            bool isInThisScreen = false;
            IntPtr thisAppMonitor;

            foreach (IntPtr hwnd in Globals.MaximizedWindows) //Итерация по списку максимизированных окон.
            {
                thisAppMonitor = (IntPtr)User32.MonitorFromWindow(hwnd, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST); //Определение монитора для текущего окна.
                if (Monitor == thisAppMonitor) { isInThisScreen = true; } //Проверка, находится ли окно на том же мониторе, что и панель задачи.
            }

            HasMaximizedWindow = isInThisScreen; //Установка свойства HasMaximizedWindow в результате проверки.
            return; //Такой оператор return; может использоваться, когда необходимо завершить выполнение метода без возвращения какого-либо значения.
                    //Он может использоваться для явного указания завершения метода, но, в данном случае, он технически не обязателен, так как метод типа void,
                    //и он завершится автоматически при достижении конца блока кода.
        }
    }

    public static class Taskbars
    {
        public static List<Taskbar> Bars { get; set; } //Статическое свойство Bars типа List<Taskbar>, представляющее список объектов Taskbar.
        public static bool MaximizedStateChanged { get; set; } //Статическое свойство MaximizedStateChanged типа bool, указывающее, изменилось ли состояние максимизации окна.
        private static string tbType; //используемое для определения типа панели задачи (максимизированной или основной).

        static Taskbars() //Конструктор класса Taskbars, вызывается при инициализации класса.
        {
            Bars = new List<Taskbar>(); //Инициализация списка Bars.
            MaximizedStateChanged = true;
        }

        public static void ApplyStyles(Taskbar taskbar) //Публичный статический метод ApplyStyles, который принимает объект Taskbar в качестве параметра.
        {
            int sizeOfPolicy = Marshal.SizeOf(taskbar.AccentPolicy); //Определение размера структуры AccentPolicy в байтах.
            IntPtr policyPtr = Marshal.AllocHGlobal(sizeOfPolicy); //Выделение памяти для структуры AccentPolicy.
            Marshal.StructureToPtr(taskbar.AccentPolicy, policyPtr, false); //Копирование данных из объекта taskbar.AccentPolicy в выделенную память.

            WindowCompositionAttribData data = new WindowCompositionAttribData(WindowsCompositionAttribute.WCA_ACCENT_POLICY, policyPtr, sizeOfPolicy); //Создание объекта WinCompatTrData с данными для установки атрибутов композиции окна.

            SWCA.SetWindowCompositionAttribute(taskbar.HWND, ref data); //Вызов внешнего метода для установки атрибутов композиции окна.

            Marshal.FreeHGlobal(policyPtr); //Освобождение выделенной памяти.
        }

        public static void UpdateMaximizedState()
        {
            foreach (Taskbar tb in Bars) //Итерация по списку панелей задач.
            {
                tb.FindMaximizedWindowsHere(); //Вызов метода FindMaximizedWindowsHere() для каждой панели задачи.
            }
            MaximizedStateChanged = false;
        }

        public static void UpdateAllSettings()
        {
            foreach (Taskbar tb in Bars) //Итерация по списку панелей задач.
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }//Определение типа панели задачи (tbType) в зависимости от наличия максимизированных окон и настроек пользователя.
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType); //Установка свойства AccentState для каждой панели задачи.
                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
                tb.AccentPolicy.GradientColor = Globals.GetTaskbarColor(tbType);
            }
        }

        public static void UpdateAccentState() //Аналогичен предыдущему методу, но обновляет только свойство AccentState.
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType);
            }
        }

        public static void UpdateAccentFlags()//Аналогичен предыдущему методу, но обновляет только свойство AccentFlags.
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && OptionsManager.Options.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
            }
        }

        public static void UpdateColor()//Аналогичен предыдущему методу, но обновляет только свойство GradientColor
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