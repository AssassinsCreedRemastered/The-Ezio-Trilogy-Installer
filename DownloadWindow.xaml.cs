using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Xml.Linq;

namespace The_Ezio_Trilogy_Installer
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        // Global Variables
        private string path { get; set; } // This is where installation directory is
        Dictionary<string, string> Sources = new Dictionary<string, string>(); // This has all of the links for mods

        public DownloadWindow()
        {
            InitializeComponent();
        }

        public DownloadWindow(string game, string installationPath)
        {
            InitializeComponent();
            if (game == "AC2")
            {
                path = installationPath;
                AC2Installation();
            }
        }

        // Universal functions
        private async Task ReadSources(string url)
        {
            try
            {
                Status.Text = "Grabbing Sources";
                Sources.Clear();
                HttpWebRequest SourceText = (HttpWebRequest)HttpWebRequest.Create(url);
                SourceText.UserAgent = "Mozilla/5.0";
                var response = SourceText.GetResponse();
                var content = response.GetResponseStream();
                using (var reader = new StreamReader(content))
                {
                    string fileContent = reader.ReadToEnd();
                    string[] lines = fileContent.Split(new char[] { '\n' });
                    foreach (string line in lines)
                    {
                        try
                        {
                            if (line != "")
                            {
                                string[] splitLine = line.Split(';');
                                Sources.Add(splitLine[0], splitLine[1]);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            continue;
                        }
                    }
                }
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        // Used to download mods from Github
        private async Task DownloadFiles(string url, string Destination)
        {
            try
            {
                Log.Information($"Downloading {System.IO.Path.GetFileNameWithoutExtension(Destination)}");
                Status.Text = "Downloading " + System.IO.Path.GetFileNameWithoutExtension(Destination);
                Progress.Value = 0;
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += WebClientDownloadProgressChanged;
                    await client.DownloadFileTaskAsync(new Uri(url), Destination);
                }
                GC.Collect();
                Log.Information("Download finished");
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        // This is used to show progress on the ProgressBar
        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
        }

        // This is used to install mods
        private async Task InstallMods(string name)
        {
            try
            {
                Log.Information($"Installing {System.IO.Path.GetFileNameWithoutExtension(name)}");
                // Directory.GetCurrenDirectory doesn't have \ at the end
                string fullPath = Directory.GetCurrentDirectory() + @"\Installation Files\" + name;
                string directory = Directory.GetCurrentDirectory() + @"\Installation Files\" + System.IO.Path.GetFileNameWithoutExtension(name);
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Installation Files\" + System.IO.Path.GetFileNameWithoutExtension(name)))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Installation Files\" + System.IO.Path.GetFileNameWithoutExtension(name));
                };
                Status.Text = "Extracting " + System.IO.Path.GetFileNameWithoutExtension(name);
                await Extract(fullPath, directory);
                Status.Text = "Installing " + System.IO.Path.GetFileNameWithoutExtension(name);
                await Move(System.IO.Path.GetFileNameWithoutExtension(name), directory);
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        // Used to extract files
        private async Task Extract(string fullPath, string Directory)
        {
            try
            {
                Log.Information($"Extracting {System.IO.Path.GetFileNameWithoutExtension(fullPath)}");
                using (var archive = ArchiveFactory.Open(fullPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(Directory, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                Log.Information($"Extracting done");
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        // Move files to game folder
        private async Task Move(string name, string Directory)
        {
            try
            {
                Log.Information($"Moving {System.IO.Path.GetFileNameWithoutExtension(name)} to game folder");
                switch (name)
                {
                    case "ASI-Loader":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            if (System.IO.File.Exists(Directory + @"\dinput8.dll"))
                            {
                                System.IO.File.Move(Directory + @"\dinput8.dll", path + @"\dinput8.dll", true);
                            }
                        }
                        if (System.IO.Directory.Exists(Directory))
                        {
                            System.IO.Directory.Delete(Directory);
                        };
                        break;
                    case "EaglePatchAC2":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            if (!System.IO.Directory.Exists(path + @"\scripts"))
                            {
                                System.IO.Directory.Move(Directory, path + @"\scripts");
                            }
                            if (System.IO.File.Exists(path + @"\scripts\Readme - EaglePatchAC2.txt"))
                            {
                                System.IO.File.Delete(path + @"\scripts\Readme - EaglePatchAC2.txt");
                            }
                        }
                        break;
                    case "uMod":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            if (!System.IO.Directory.Exists(path + @"\uMod"))
                            {
                                System.IO.Directory.Move(Directory, path + @"\uMod");
                            }
                        }
                        break;
                    case "PSButtons":
                        if (!System.IO.Directory.Exists(path + @"\Mods"))
                        {
                            System.IO.Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!System.IO.Directory.Exists(path + @"\Mods\PS3Buttons"))
                        {
                            System.IO.Directory.Move(Directory, path + @"\Mods\PS3Buttons");
                        }
                        break;
                    case "PCButtons":
                        if (!System.IO.Directory.Exists(path + @"\Mods"))
                        {
                            System.IO.Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!System.IO.Directory.Exists(path + @"\Mods\PCButtons"))
                        {
                            System.IO.Directory.Move(Directory, path + @"\Mods\PCButtons");
                        }
                        break;
                    case "RecognizableFeather":
                        if (!System.IO.Directory.Exists(path + @"\Mods"))
                        {
                            System.IO.Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!System.IO.Directory.Exists(path + @"\Mods\Recognizable Feathers\"))
                        {
                            System.IO.Directory.Move(Directory, path + @"\Mods\Recognizable Feathers\");
                        }
                        break;
                    case "ReShade":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            foreach (string file in System.IO.Directory.GetFiles(Directory))
                            {
                                if (!System.IO.File.Exists(path + @"\" + System.IO.Path.GetFileName(file)))
                                {
                                    System.IO.File.Move(file, path + @"\" + System.IO.Path.GetFileName(file), true);
                                }
                            }
                            foreach (string dir in System.IO.Directory.GetDirectories(Directory))
                            {
                                if (!System.IO.Directory.Exists(path + @"\" + System.IO.Path.GetFileName(dir)))
                                {
                                    System.IO.Directory.Move(dir, path + @"\" + System.IO.Path.GetFileName(dir));
                                }
                            }
                        }
                        break;
                    case "Overhaul":
                        if (!System.IO.Directory.Exists(path + @"\Mods"))
                        {
                            System.IO.Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!System.IO.Directory.Exists(path + @"\Mods\Overhaul"))
                        {
                            System.IO.Directory.Move(Directory, path + @"\Mods\Overhaul");
                        }
                        break;
                    case "Launcher":
                        break;
                    default:
                        break;
                }
                Log.Information($"Moving done");
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        // Used to setup uMod
        private async Task uModSetup(string gameName)
        {
            try
            {
                Log.Information("Setting up uMod");
                await uModAppData(gameName);
                await Task.Delay(10);
                await uModSaveFile(gameName);
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        private async Task uModAppData(string gameName)
        {
            try
            {
                Log.Information("Setting up uMod AppData config so it can detect the game");
                string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!Directory.Exists(AppData + @"\uMod"))
                {
                    Log.Information("No old uMod config detected. Doing fresh setup");
                    Directory.CreateDirectory(AppData + @"\uMod");
                    string ExecutableDirectory = path + @"\" + gameName;
                    char[] array = ExecutableDirectory.ToCharArray();
                    List<char> charList = new List<char>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i == 0)
                        {
                            charList.Add(array[i]);
                        }
                        else
                        {
                            charList.Add('\0');
                            charList.Add(array[i]);
                        }
                    }
                    charList.Add('\0');
                    char[] charArray = charList.ToArray();
                    string ExecutablePath = new string(charArray);
                    using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9.txt"))
                    {
                        sw.Write(ExecutablePath);
                    }

                }
                else
                {
                    if (!File.Exists(AppData + @"\uMod\uMod_DX9.txt"))
                    {
                        Log.Information("No old uMod config detected. Doing fresh setup");
                        Directory.CreateDirectory(AppData + @"\uMod");
                        string ExecutableDirectory = path + @"\" + gameName;
                        char[] array = ExecutableDirectory.ToCharArray();
                        List<char> charList = new List<char>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (i == 0)
                            {
                                charList.Add(array[i]);
                            }
                            else
                            {
                                charList.Add('\0');
                                charList.Add(array[i]);
                            }
                        }
                        charList.Add('\0');
                        char[] charArray = charList.ToArray();
                        string ExecutablePath = new string(charArray);
                        using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9.txt"))
                        {
                            sw.Write(ExecutablePath);
                        }
                    }
                    else
                    {
                        Log.Information("Old uMod config detected. Adding to it.");
                        Directory.CreateDirectory(AppData + @"\uMod");
                        string ExecutableDirectory = path + @"\" + gameName;
                        char[] array = ExecutableDirectory.ToCharArray();
                        List<char> charList = new List<char>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (i == 0)
                            {
                                charList.Add(array[i]);
                            }
                            else
                            {
                                charList.Add('\0');
                                charList.Add(array[i]);
                            }
                        }
                        charList.Add('\0');
                        char[] charArray = charList.ToArray();
                        string ExecutablePath = new string(charArray);
                        using (StreamReader sr = new StreamReader(AppData + @"\uMod\uMod_DX9.txt"))
                        {
                            using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9temp.txt"))
                            {
                                string line = sr.ReadLine();
                                while (line != null)
                                {

                                    if (line == '\0'.ToString())
                                    {
                                        sw.Write(line);
                                    }
                                    else
                                    {
                                        sw.Write(line + "\n");
                                    }
                                    line = sr.ReadLine();
                                }
                                sw.Write(ExecutablePath);
                            }
                        }
                        File.Delete(AppData + @"\uMod\uMod_DX9.txt");
                        File.Move(AppData + @"\uMod\uMod_DX9temp.txt", AppData + @"\uMod\uMod_DX9.txt");
                    }
                }
                Log.Information("Setting up uMod AppData config done");
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        private async Task uModSaveFile(string gameName)
        {
            try
            {
                if (!System.IO.Directory.Exists(path + @"\uMod\templates"))
                {
                    System.IO.Directory.CreateDirectory(path + @"\uMod\templates");
                }
                switch (gameName)
                {
                    case "AssassinsCreedIIGame.exe":
                        Log.Information("Setting up uMod for Assassin's Creed 2");
                        using (StreamWriter sw = new StreamWriter(path + @"\uMod\templates\ac2.txt"))
                        {
                            sw.Write("SaveAllTextures:0\n");
                            sw.Write("SaveSingleTexture:0\n");
                            sw.Write("FontColour:255,0,0\n");
                            sw.Write("TextureColour:0,255,0\n");
                            sw.Write("Add_true:" + path + @"\Mods\Overhaul\Overhaul.tpf" + "\n");
                            sw.Write("Add_true:" + path + @"\Mods\PCButtons\PC Buttons.tpf" + "\n");
                        }
                        string saveFile = path + @"\" + gameName + "|" + path + @"\uMod\templates\ac2.txt";
                        char[] array = saveFile.ToCharArray();
                        List<char> charList = new List<char>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (i == 0)
                            {
                                charList.Add(array[i]);
                            }
                            else
                            {
                                charList.Add('\0');
                                charList.Add(array[i]);
                            }
                        }
                        charList.Add('\0');
                        char[] charArray = charList.ToArray();
                        string SaveFilePATH = new string(charArray);
                        using (StreamWriter sw = new StreamWriter(path + @"\uMod\uMod_SaveFiles.txt"))
                        {
                            sw.Write(SaveFilePATH);
                        }
                        Log.Information("Setting up uMod for Assassin's Creed 2 done");
                        break;
                    default:
                        break;
                }
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        // Assasssin's Creed 2 Installation Specific
        private async void AC2Installation()
        {
            try
            {
                // Creating folder where all of the temporary files will be stored
                if (!System.IO.Directory.Exists("Installation Files"))
                {
                    Log.Information("Installation Files folder not found.");
                    System.IO.Directory.CreateDirectory("Installation Files");
                    Log.Information("Installation Files folder created.");
                }

                // This is where path to the install will be located
                if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered"))
                {
                    Log.Information("Folder where all paths to game installation folders go not found.");
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered");
                    Log.Information("Folder where all paths to game installation folders created");
                }

                // This is where custom mods will go
                if (!System.IO.Directory.Exists(path + @"\Mods\Custom Mods"))
                {
                    System.IO.Directory.CreateDirectory(path + @"\Mods\Custom Mods");
                }

                Log.Information("Writing path towards Assassin's Creed 2 installation folder inside of AC2Path.txt");
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\AC2Path.txt"))
                {
                    sw.WriteLine(path);
                };
                // First need to read Sources.txt to get all of the download links
                Log.Information("Reading Mods download links from the Source");
                
                await ReadSources("https://raw.githubusercontent.com/AssassinsCreedRemastered/The-Ezio-Trilogy-Mods/main/AC2Sources.txt");
                // For every download link we need to download it and then install it
                for (int i = 0; i < Sources.Keys.Count; i++)
                {
                    KeyValuePair<string, string> keyValue = Sources.ElementAt(i);
                    if (!System.IO.File.Exists(Directory.GetCurrentDirectory() + @"\Installation Files\" + keyValue.Key))
                    {
                        await DownloadFiles(keyValue.Value, @"Installation Files\" + keyValue.Key);
                    }
                    await InstallMods(keyValue.Key);
                };
                Status.Text = "Setting up uMod";
                await uModSetup("AssassinsCreedIIGame.exe");
                Log.Information("Cleaning up");
                Status.Text = "Cleaning up";
                System.IO.Directory.Delete(Directory.GetCurrentDirectory() + @"\Installation Files", true);
                Log.Information("Installation Complete");
                await Task.Delay(10);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }
    }
}
