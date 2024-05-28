using System;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace Extensions
{
    #region Enums
    public enum AccentState //Это перечисление, представляющее различные состояния акцентирования (цветовой схемы) в Windows.
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    public enum WindowsCompositionAttribute // перечисление которое связано с атрибутами композиции окна
    {
        WCA_ACCENT_POLICY = 19
    }
    #endregion

    #region Structs
    [StructLayout(LayoutKind.Sequential)] // здесь указывается как должны храниться в памяти элементы структуры,
                                          // в данном случае они будут распологаться в порядке объявления
    public struct AccentPolicy // эта структура представляет параметры акцентирования(цветовой схемы) в windows
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationID;

        public AccentPolicy(AccentState accentState, int accentFlags, int gradientColor, int animationID)
        {
            AccentState = accentState;
            AccentFlags = accentFlags;
            GradientColor = gradientColor;
            AnimationID = animationID;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttribData // эта структура связана с данными о совместимости и прозрачности окна
    {
        public WindowsCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;

        public WindowCompositionAttribData(WindowsCompositionAttribute attribute, IntPtr data, int sizeOfData)
        {
            Attribute = attribute;
            Data = data;
            SizeOfData = sizeOfData;
        }
    }
    #endregion


    public static class SWCA
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowCompositionAttribute(HWND hwnd, ref WindowCompositionAttribData data); //Этот метод используется для установки атрибутов композиции окна, таких как акцент и прозрачность.
    }                                                                                                              //Он вызывает функцию SetWindowCompositionAttribute из user32.dll и принимает структуру WinCompatTrData
                                                                                                                   //в качестве параметра.

}
