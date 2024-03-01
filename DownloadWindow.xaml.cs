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
using IWshRuntimeLibrary;
using System.Windows.Forms;
using System.Windows.Markup.Localizer;
using System.Net.Http;

namespace The_Ezio_Trilogy_Installer
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        // Global Variables
        private string? path { get; set; } // This is where installation directory is
        Dictionary<string, string> Sources = new Dictionary<string, string>(); // This has all of the links for mods

        public DownloadWindow()
        {
            InitializeComponent();
        }

        public DownloadWindow(string game, string installationPath)
        {
            InitializeComponent();
            path = installationPath;
            switch (game)
            {
                case "AC2":
                    AC2Installation();
                    break;
                case "ACB":
                    ACBInstallation();
                    break;
                case "ACR":
                    ACRInstallation();
                    break;
                default:
                    break;
            }
        }
        // Universal functions
        /// <summary>
        /// Reads sources from a text file on Github repo and stores them into Dictionary
        /// </summary>
        /// <param name="url">URL where text file is stored.</param>
        private async Task ReadSources(string url)
        {
            try
            {
                Status.Text = "Grabbing Sources";
                Sources.Clear();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

                    var response = await client.GetStringAsync(url);
                    var lines = response.Split(new char[] { '\n' });

                    foreach (var line in lines)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                var splitLine = line.Split(';');
                                Sources.Add(splitLine[0], splitLine[1]);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.ToString());
                            continue;
                        }
                    }
                }

                GC.Collect();
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error:");
                return;
            }
        }
        /// <summary>
        /// Downloads mods from Github backup repository
        /// </summary>
        /// <param name="url">URL where text file is stored.</param>
        /// <param name="Destination">Path where downloaded files are stored.</param>
        private async Task DownloadFiles(string url, string destination)
        {
            try
            {
                Log.Information($"Downloading {System.IO.Path.GetFileNameWithoutExtension(destination)}");
                Status.Text = "Downloading " + System.IO.Path.GetFileNameWithoutExtension(destination);
                Progress.Value = 0;

                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var totalBytes = response.Content.Headers.ContentLength ?? -1;
                            var buffer = new byte[8192];
                            var bytesRead = 0;
                            var totalRead = 0;
                            do
                            {
                                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                                    totalRead += bytesRead;

                                    // Calculate progress percentage
                                    var progressPercentage = totalBytes == -1 ? 0 : (int)((double)totalRead / totalBytes * 100);
                                    Progress.Value = progressPercentage;
                                }
                            } while (bytesRead > 0);
                        }
                    }
                }

                Log.Information("Download finished");
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error:");
                return;
            }
        }

        /// <summary>
        /// Installs downloaded mods
        /// </summary>
        /// <param name="name">Name of the mod folder</param>
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

        /// <summary>
        /// Extracts the zip of mods
        /// </summary>
        /// <param name="fullPath">Full path to the zip file.</param>
        /// <param name="Directory">Where it'll be extracted to.</param>
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

        /// <summary>
        /// Move files to game folder
        /// </summary>
        /// <param name="name">Name of the mod folder.</param>
        /// <param name="Directory">Where it'll be moved to.</param>
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
                                System.IO.File.Copy(Directory + @"\dinput8.dll", path + @"\dinput8.dll", true);
                            }
                        }
                        break;
                    case "EaglePatchAC2":
                        if(!System.IO.Directory.Exists(path + @"\scripts\"))
                        {
                            System.IO.Directory.CreateDirectory(path + @"\scripts\");
                        }
                        if (System.IO.Directory.Exists(Directory))
                        {
                            System.IO.File.Copy(Directory + @"\EaglePatchAC2.asi", path + @"\scripts\EaglePatchAC2.asi", true);
                            System.IO.File.Copy(Directory + @"\EaglePatchAC2.ini", path + @"\scripts\EaglePatchAC2.ini", true);
                            await DisableUnlockingRewards();
                        }
                        break;
                    case "Launcher":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            if (System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\"))
                            {
                                if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher.exe"))
                                {
                                    System.IO.File.Copy(Directory + @"\The Ezio Trilogy Launcher.exe", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher.exe", true);
                                }
                            }
                        }
                        break;
                    case "Updater":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            if (System.IO.File.Exists(Directory + @"\The Ezio Trilogy Launcher Updater.exe"))
                            {
                                System.IO.File.Copy(Directory + @"\The Ezio Trilogy Launcher Updater.exe", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher Updater.exe", true);
                            }
                        }
                        break;
                    case "uMod":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            if (!System.IO.Directory.Exists(path + @"\uMod"))
                            {
                                CopyDirectory(Directory, path + @"\uMod");
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
                            CopyDirectory(Directory, path + @"\Mods\PS3Buttons");
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
                    case "ReShade":
                        if (System.IO.Directory.Exists(Directory))
                        {
                            CopyDirectory(Directory, path + @"\");
                        }
                        break;
                    case "Overhaul":
                        if (!System.IO.Directory.Exists(path + @"\Mods"))
                        {
                            System.IO.Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!System.IO.Directory.Exists(path + @"\Mods\Overhaul"))
                        {
                            CopyDirectory(Directory, path + @"\Mods\Overhaul");
                        }
                        break;
                    default:
                        break;
                }
                Log.Information($"Moving done");
                GC.Collect();
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        /// <summary>
        /// Used to copy directories (Directory.Move doesn't work over different partitions)
        /// </summary>
        private static void CopyDirectory(string source, string target)
        {
            try
            {
                if (!Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                }
                foreach (string file in Directory.GetFiles(source))
                {
                    string destination = System.IO.Path.Combine(target, System.IO.Path.GetFileName(file));
                    System.IO.File.Copy(file, destination, true);
                }

                foreach (string subDir in Directory.GetDirectories(source))
                {
                    string destination = System.IO.Path.Combine(target, System.IO.Path.GetFileName(subDir));
                    CopyDirectory(subDir, destination);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Disable unlocking Uplay Rewards in EaglePatch (Better method implemented)
        /// </summary>
        private async Task DisableUnlockingRewards()
        {
            try
            {
                if (System.IO.File.Exists(path + @"\scripts\EaglePatchAC2.ini"))
                {
                    using (StreamReader sr = new StreamReader(path + @"\scripts\EaglePatchAC2.ini"))
                    {
                        using (StreamWriter sw = new StreamWriter(path + @"\scripts\EaglePatchAC2temp.ini"))
                        {
                            string? line = sr.ReadLine();
                            while (line != null)
                            {
                                if (line.StartsWith("UPlayItems"))
                                {
                                    sw.Write("UPlayItems=0");
                                } else
                                {
                                    sw.WriteLine(line);
                                }
                                line = sr.ReadLine();
                            }
                        }
                    }
                    System.IO.File.Delete(path + @"\scripts\EaglePatchAC2.ini");
                    System.IO.File.Move(path + @"\scripts\EaglePatchAC2temp.ini", path + @"\scripts\EaglePatchAC2.ini");
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Information(ex, "");
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Used to setup uMod
        /// </summary>
        /// <param name="gameName">Name of the game.</param>
        private async Task uModSetup(string gameName)
        {
            try
            {
                Log.Information("Setting up uMod");
                await uModAppData(gameName);
                await Task.Delay(1);
                await uModSaveFile(gameName);
                GC.Collect();
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        /// <summary>
        /// Adds the game to the AppData uMod file
        /// </summary>
        /// <param name="gameName">Name of the game.</param>
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
                    if (!System.IO.File.Exists(AppData + @"\uMod\uMod_DX9.txt"))
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
                                string? line = sr.ReadLine();
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
                        System.IO.File.Delete(AppData + @"\uMod\uMod_DX9.txt");
                        System.IO.File.Move(AppData + @"\uMod\uMod_DX9temp.txt", AppData + @"\uMod\uMod_DX9.txt");
                    }
                }
                Log.Information("Setting up uMod AppData config done");
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        /// <summary>
        /// Sets up the game in uMod Template file
        /// </summary>
        /// <param name="gameName">Name of the game.</param>
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
                        if (!System.IO.File.Exists(path + @"\uMod\Status.txt"))
                        {
                            using (StreamWriter sw = new StreamWriter(path + @"\uMod\Status.txt"))
                            {
                                sw.Write("Enabled=1");
                            }
                        }
                        Log.Information("Setting up uMod for Assassin's Creed 2");
                        using (StreamWriter sw = new StreamWriter(path + $@"\uMod\templates\ac2.txt"))
                        {
                            sw.Write("SaveAllTextures:0\n");
                            sw.Write("SaveSingleTexture:0\n");
                            sw.Write("FontColour:255,0,0\n");
                            sw.Write("TextureColour:0,255,0\n");
                            sw.Write("Add_true:" + path + @"\Mods\Overhaul\Overhaul.tpf" + "\n");
                        }
                        string AC2saveFile = path + @"\" + gameName + "|" + path + $@"\uMod\templates\ac2.txt";
                        char[] AC2array = AC2saveFile.ToCharArray();
                        List<char> AC2charList = new List<char>();
                        for (int i = 0; i < AC2array.Length; i++)
                        {
                            if (i == 0)
                            {
                                AC2charList.Add(AC2array[i]);
                            }
                            else
                            {
                                AC2charList.Add('\0');
                                AC2charList.Add(AC2array[i]);
                            }
                        }
                        AC2charList.Add('\0');
                        char[] AC2charArray = AC2charList.ToArray();
                        string AC2SaveFilePATH = new string(AC2charArray);
                        using (StreamWriter sw = new StreamWriter(path + @"\uMod\uMod_SaveFiles.txt"))
                        {
                            sw.Write(AC2SaveFilePATH);
                        }
                        Log.Information("Setting up uMod for Assassin's Creed 2 done");
                        break;
                    case "ACBSP.exe":
                        Log.Information("Setting up uMod for Assassin's Creed Brotherhood");
                        if (!System.IO.File.Exists(path + @"\uMod\Status.txt"))
                        {
                            using (StreamWriter sw = new StreamWriter(path + @"\uMod\Status.txt"))
                            {
                                sw.Write("Enabled=1");
                            }
                        }
                        using (StreamWriter sw = new StreamWriter(path + $@"\uMod\templates\{System.IO.Path.GetFileNameWithoutExtension(gameName)}.txt"))
                        {
                            sw.Write("SaveAllTextures:0\n");
                            sw.Write("SaveSingleTexture:0\n");
                            sw.Write("FontColour:255,0,0\n");
                            sw.Write("TextureColour:0,255,0\n");
                            sw.Write("Add_true:" + path + @"\Mods\PS3Buttons\ACB PS Buttons.tpf" + "\n");
                        }
                        string ACBsaveFile = path + @"\" + gameName + "|" + path + $@"\uMod\templates\{System.IO.Path.GetFileNameWithoutExtension(gameName)}.txt";
                        char[] ACBarray = ACBsaveFile.ToCharArray();
                        List<char> ACBcharList = new List<char>();
                        for (int i = 0; i < ACBarray.Length; i++)
                        {
                            if (i == 0)
                            {
                                ACBcharList.Add(ACBarray[i]);
                            }
                            else
                            {
                                ACBcharList.Add('\0');
                                ACBcharList.Add(ACBarray[i]);
                            }
                        }
                        ACBcharList.Add('\0');
                        char[] ACBcharArray = ACBcharList.ToArray();
                        string ACBSaveFilePATH = new string(ACBcharArray);
                        using (StreamWriter sw = new StreamWriter(path + @"\uMod\uMod_SaveFiles.txt"))
                        {
                            sw.Write(ACBSaveFilePATH);
                        }
                        Log.Information("Setting up uMod for Assassin's Creed Brotherhood done");
                        break;
                    default:
                        break;
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        /// <summary>
        /// Creates the Shortcut in Search and on Desktop
        /// </summary>
        private async Task CreateShortcut()
        {
            try
            {
                if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk"))
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to create shortcut?", "Confirmation", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        Log.Information("Creating shortcuts");
                        WshShell shell = new WshShell();
                        IWshShortcut shortcut = shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk");
                        shortcut.Description = "Shortcut for Assassin's Creed - The Ezio Trilogy Remastered (Community Edition) Launcher";
                        shortcut.IconLocation = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher.exe"},{0}";
                        shortcut.TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\The Ezio Trilogy Launcher.exe";
                        shortcut.Save();
                        System.IO.File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Assassin's Creed - The Ezio Trilogy Remastered.lnk");
                    } else
                    {
                        Log.Information("Skipping creation of shortcuts");
                    }
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Used for installing everything for Assassin's Creed 2
        /// </summary>
        private async void AC2Installation()
        {
            try
            {
                Log.Information("Assassin's Creed 2 installation started");
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

                // Path towards executable
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

                await CreateShortcut();
                Log.Information("Cleaning up");
                Status.Text = "Cleaning up";
                System.IO.Directory.Delete(Directory.GetCurrentDirectory() + @"\Installation Files", true);
                Log.Information("Installation Complete");
                await Task.Delay(1);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        /// <summary>
        /// Used for installing everything for Assassin's Creed Brotherhood
        /// </summary>
        private async void ACBInstallation()
        {
            try
            {
                Log.Information("Assassin's Creed Brotherhood installation started");
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

                // Path towards executable
                Log.Information("Writing path towards Assassin's Creed Brotherhood installation folder inside of ACBPath.txt");
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\ACBPath.txt"))
                {
                    sw.WriteLine(path);
                };

                // First need to read Sources.txt to get all of the download links
                Log.Information("Reading Mods download links from the Source");
                await ReadSources("https://raw.githubusercontent.com/AssassinsCreedRemastered/The-Ezio-Trilogy-Mods/main/ACBSources.txt");
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
                await uModSetup("ACBSP.exe");
                await CreateShortcut();
                Status.Text = "Cleaning up";
                Log.Information("Cleaning up");
                System.IO.Directory.Delete(Directory.GetCurrentDirectory() + @"\Installation Files", true);
                Log.Information("Installation Complete");
                await Task.Delay(1);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                return;
            }
        }

        /// <summary>
        /// Used for installing everything for Assassin's Creed Revelations
        /// </summary>
        private async void ACRInstallation()
        {
            try
            {
                Log.Information("Assassin's Creed Revelations installation started");
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

                // Path towards executable
                Log.Information("Writing path towards Assassin's Creed Revelations installation folder inside of ACRPath.txt");
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\ACRPath.txt"))
                {
                    sw.WriteLine(path);
                };

                // First need to read Sources.txt to get all of the download links
                Log.Information("Reading Mods download links from the Source");
                await ReadSources("https://raw.githubusercontent.com/AssassinsCreedRemastered/The-Ezio-Trilogy-Mods/main/ACRSources.txt");
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

                // Disabling "systemdetection.dll"
                if (System.IO.File.Exists(path + @"\systemdetection.dll"))
                {
                    System.IO.File.Move(path + @"\systemdetection.dll", path + @"\systemdetection.dll.disabled", true);
                }

                await CreateShortcut();
                Status.Text = "Cleaning up";
                Log.Information("Cleaning up");
                System.IO.Directory.Delete(Directory.GetCurrentDirectory() + @"\Installation Files", true);
                Log.Information("Installation Complete");
                await Task.Delay(1);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}
