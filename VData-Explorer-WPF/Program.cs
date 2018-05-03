using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.ApplicationServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VData_Explorer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyLoader.AssemblyResolve;

            Controller controller = new Controller();
            controller.Run(args);
        }

        class Controller : WindowsFormsApplicationBase
        {
            public Controller() : base(AuthenticationMode.Windows)
            {
                
            }

            protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
            {
                try
                {
                    App app = new App();
                    app.Run();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return false;
            }
        }
    }
}
