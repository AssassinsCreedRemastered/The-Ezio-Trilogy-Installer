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

        private string? path {  get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Log.Information("Application loaded");
        }

        private async Task RemoveGameFromuMod(string gameName)
        {
            try
            {
                string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                bool isFirstLine = true;
                using (StreamReader sr = new StreamReader(AppData + @"\uMod\uMod_DX9.txt"))
                {
                    using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9temp.txt"))
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            if (line == '\0'.ToString() && !line.EndsWith(gameName))
                            {
                                if (isFirstLine)
                                {
                                    sw.Write(line.TrimStart('\0'));
                                    isFirstLine = false;
                                } else
                                {
                                    sw.Write(line);
                                }
                            }
                            else if (!line.EndsWith(gameName))
                            {
                                if (isFirstLine)
                                {
                                    sw.Write(line.TrimStart('\0') + "\n");
                                    isFirstLine = false;
                                }
                                else
                                {
                                    sw.Write(line + "\n");
                                }
                            }
                            line = sr.ReadLine();
                        }
                    }
                }
                File.Delete(AppData + @"\uMod\uMod_DX9.txt");
                File.Move(AppData + @"\uMod\uMod_DX9temp.txt", AppData + @"\uMod\uMod_DX9.txt");
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
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
                FileDialog.Title = "Select Assassins Creed 2 Executable";
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
                MessageBox.Show("Installation done");
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error:");
                return;
            }
        }

        private async void InstallACB_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Installing Assassin's Creed Brotherhood");
            try
            {
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "Executable Files|ACBSP.exe";
                FileDialog.Title = "Select Assassins Creed - Brotherhood Executable";
                if (FileDialog.ShowDialog() == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                }
                else
                {
                    return;
                }
                DownloadWindow download = new DownloadWindow("ACB", path);
                if (download == null || !download.IsVisible)
                {
                    download.ShowDialog();
                }
                else
                {
                    download.ShowDialog();
                }
                MessageBox.Show("Installation done");
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error:");
                return;
            }
        }

        private void InstallACR_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UninstallACII_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Uninstall AC2");
            try
            {
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "Executable Files|AssassinsCreedIIGame.exe";
                FileDialog.Title = "Select Assassins Creed 2 Executable";
                if (FileDialog.ShowDialog() == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                }
                else
                {
                    Log.Information("Uninstallation cancelled");
                    MessageBox.Show("Unninstallation cancelled");
                    return;
                }
                
                // Delete Mods Folder that has all of the uMod mods
                Log.Information("Removing uMod mods folder");
                if (System.IO.Directory.Exists(path + @"\Mods"))
                {
                    System.IO.Directory.Delete(path + @"\Mods", true);
                }

                // Delete Ultimate ASI Loader
                Log.Information("Removing ASI Loader");
                if (System.IO.File.Exists(path + @"\dinput8.dll"))
                {
                    System.IO.File.Delete(path + @"\dinput8.dll");
                };

                // Delete scripts folder that has EaglePatch
                Log.Information("Removing EaglePatch");
                if (System.IO.Directory.Exists(path + @"\scripts"))
                {
                    System.IO.Directory.Delete(path + @"\scripts", true);
                };

                // Delete uMod
                Log.Information("Removing uMod");
                if (System.IO.Directory.Exists(path + @"\uMod"))
                {
                    System.IO.Directory.Delete(path + @"\uMod", true);
                }
              
                // Asking if uMod settings want to be deleted
                MessageBoxResult result = MessageBox.Show("Do you want to delete all of uMod settings?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    // Delete uMod settings
                    Log.Information("Removing uMod settings");
                    if (System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\uMod"))
                    {
                        System.IO.Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\uMod", true);
                    }
                } else if(result == MessageBoxResult.No)
                {
                    Log.Information("Removing the game from uMod settings");
                    RemoveGameFromuMod("AssassinsCreedIIGame.exe");
                }
                // Removing path
                Log.Information("Removing txt file containing game path towards the game");
                if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\AC2Path.txt"))
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\AC2Path.txt");
                }

                MessageBoxResult LauncherDeletion = System.Windows.MessageBox.Show("Do you want to delete the launcher?", "Confirmation", MessageBoxButton.YesNo);
                if (LauncherDeletion == MessageBoxResult.Yes)
                {
                    if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher.exe"))
                    {
                        System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher.exe");
                    }
                    if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk"))
                    {
                        System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk");
                    }
                    if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk"))
                    {
                        System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk");
                    }
                }

                // Checking if Documents folder containing all of the Paths towards game installation folders is empty, if it is remove it
                Log.Information("Checking if Documents folder containing all of the Paths towards game installation folders exist");
                if (System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\"))
                {
                    Log.Information("Checking if Documents folder containing all of the Paths towards game installation folders is empty");
                    if (Directory.EnumerateFileSystemEntries(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\").Count() == 0)
                    {
                        Log.Information("Documents folder");
                        System.IO.Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\");
                    } else
                    {
                        Log.Information("Folder not empty.");
                    }
                }

                // Delete ReShade
                Log.Information("Removing ReShade");
                if (System.IO.File.Exists(path + @"\d3d9.dll"))
                {
                    System.IO.File.Delete(path + @"\d3d9.dll");
                };
                if (System.IO.File.Exists(path + @"\dxgi.dll"))
                {
                    System.IO.File.Delete(path + @"\dxgi.dll");
                };
                if (System.IO.File.Exists(path + @"\d3d9.log"))
                {
                    System.IO.File.Delete(path + @"\d3d9.log");
                };
                if (System.IO.File.Exists(path + @"\ReShade.ini"))
                {
                    System.IO.File.Delete(path + @"\ReShade.ini");
                };
                if (System.IO.File.Exists(path + @"\ReShade.log"))
                {
                    System.IO.File.Delete(path + @"\ReShade.log");
                };
                if (System.IO.Directory.Exists(path + @"\reshade-presets"))
                {
                    System.IO.Directory.Delete(path + @"\reshade-presets", true);
                };
                if (System.IO.Directory.Exists(path + @"\reshade-shaders"))
                {
                    System.IO.Directory.Delete(path + @"\reshade-shaders", true);
                };
                Log.Information("Uninstallation done");
                MessageBox.Show("Uninstallation done");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        private void UninstallACB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "Executable Files|ACBSP.exe";
                FileDialog.Title = "Select Assassins Creed Brotherhood Executable";
                if (FileDialog.ShowDialog() == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                }
                else
                {
                    Log.Information("Uninstallation cancelled");
                    MessageBox.Show("Unninstallation cancelled");
                    return;
                }

                // Delete Mods Folder that has all of the uMod mods
                Log.Information("Removing uMod mods folder");
                if (System.IO.Directory.Exists(path + @"\Mods"))
                {
                    System.IO.Directory.Delete(path + @"\Mods", true);
                }

                // Delete Ultimate ASI Loader
                Log.Information("Removing ASI Loader");
                if (System.IO.File.Exists(path + @"\dinput8.dll"))
                {
                    System.IO.File.Delete(path + @"\dinput8.dll");
                };

                // Delete scripts folder that has EaglePatch
                Log.Information("Removing EaglePatch");
                if (System.IO.Directory.Exists(path + @"\scripts"))
                {
                    System.IO.Directory.Delete(path + @"\scripts", true);
                };

                // Delete uMod
                Log.Information("Removing uMod");
                if (System.IO.Directory.Exists(path + @"\uMod"))
                {
                    System.IO.Directory.Delete(path + @"\uMod", true);
                }

                // Removing path
                Log.Information("Removing txt file containing game path towards the game");
                if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\ACBPath.txt"))
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\ACBPath.txt");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}
