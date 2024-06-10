using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WiPapper
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void AccentComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //WallpaperButton.IsEnabled = false;
            //TaskBarButton.IsEnabled = true;
        }

        private void UseMaximizedSettingsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            //TaskBarButton.IsEnabled = false;
            //WallpaperButton.IsEnabled = true;
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            
        }
        private void ColorPicker_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            
        }
        private void WindowsAccentColorCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            
        }
        private void ColorizeCB_Changed(object sender, RoutedEventArgs e)
        {
            
        }
        private void AccentAlphaSlider_DragCompleted(object sender, RoutedEventArgs e)
        {
            
        }
        private void AccentAlphaSlider_DragStarted(object sender, RoutedEventArgs e)
        {
            
        }
        private void AccentAlphaSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void SelectWall_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetWallpaperButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnSetWallpaper_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Volume_ValueChanged(object sender, RoutedEventArgs e)
        {

        }
        private void WallpaperStretchTypeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }


    }
}
