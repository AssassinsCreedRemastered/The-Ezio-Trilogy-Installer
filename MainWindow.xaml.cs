using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
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

// Imported
using Microsoft.Win32;

namespace The_Ezio_Trilogy_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string path {  get; set; }

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

        private async void InstallACII_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Installing AC2");
            try
            {
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "Executable Files|AssassinsCreedIIGame.exe";
                FileDialog.Title = "Select an Assassins Creed 2 Executable";
                if (FileDialog.ShowDialog() == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                }
                else
                {
                    return;
                }
                DownloadWindow download = new DownloadWindow("AC2", path);
                if (download == null || !download.IsVisible)
                {
                    download.ShowDialog();
                } else
                {
                    download.ShowDialog();
                }
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error:");
                return;
            }
        }

        private void InstallACB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InstallACR_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
