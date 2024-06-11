using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WiPapper
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Логирование исключения
            Debug.WriteLine(e.Exception.Message);
            Debug.WriteLine(e.Exception.StackTrace);
            if (e.Exception.InnerException != null)
            {
                Debug.WriteLine(e.Exception.InnerException.Message);
                Debug.WriteLine(e.Exception.InnerException.StackTrace);
            }

            // Предотвращение завершения работы приложения
            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Освобождение ресурсов и завершение фоновых задач
            // ...
        }
    }

}
