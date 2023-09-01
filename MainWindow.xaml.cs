using Serilog;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace The_Ezio_Trilogy_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log.Information("Application loaded");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Exiting the installer");
            Environment.Exit(0);
        }

        private void InstallACII_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Installing AC2");
        }

        private void InstallACB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InstallACR_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
