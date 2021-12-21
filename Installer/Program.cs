using System;
using System.IO;
using System.Windows.Forms;
using Ookii.Dialogs;
using SatelliteLoader;

namespace Installer {
    internal class Program {
        public static string Folder = FindGameFolder("A Dance of Fire and Ice");
        [STAThread] public static void Main(string[] args) {
            var version = typeof(Satellite).Assembly.GetName().Version.ToString();
            Console.Title = $"Satellite Installer";
            while (true) {
                Console.Clear();
                Console.WriteLine($"Satellite v{version}\n");
                Console.WriteLine("1: Select folder\n2: Install Satellite\n3: Uninstall Satellite\n4: Restore files\n5: Exit");
                Console.WriteLine("");
                Console.WriteLine($"Selected Folder: {Folder}");
                switch (Console.ReadKey().Key) {
                    case ConsoleKey.D1:
                        var dialog = new VistaFolderBrowserDialog();
                        dialog.SelectedPath = Folder;
                        //dialog.Filter = "A Dance of Fire and Ice.exe|A Dance of Fire and Ice.exe";
                        //dialog.Title = "Select Folder";
                        if (dialog.ShowDialog() == DialogResult.OK) {
                            Folder = dialog.SelectedPath; // Path.GetDirectoryName(dialog.FileName);
                        }

                        continue;

                    case ConsoleKey.D2:
                        Console.WriteLine("\n");
                        Console.WriteLine("Installing Satellite...");
                        if (Injector.Init(Injector.Actions.Install)) {
                            Console.WriteLine("Installation Success!");
                        } else {
                            Console.WriteLine("Installation Failed.");
                        }

                        Console.ReadKey();
                        continue;

                    case ConsoleKey.D3:
                        Console.WriteLine("\n");
                        Console.WriteLine("Uninstalling Satellite...");
                        if (Injector.Init(Injector.Actions.Delete)) {
                            Console.WriteLine("Uninstallation success!");
                        } else {
                            Console.WriteLine("Uninstallation failed.");
                        }

                        Console.ReadKey();
                        continue;

                    case ConsoleKey.D4:
                        Console.WriteLine("\n");
                        Console.WriteLine("Restoring game files...");
                        if (Injector.Init(Injector.Actions.Restore)) {
                            Console.WriteLine("Restore success!");
                        } else {
                            Console.WriteLine("Restore failed.");
                        }

                        Console.ReadKey();
                        continue;
                    
                    case ConsoleKey.D5:
                        goto Exit;

                    default:
                        continue;
                }
            }
            Exit: ;
        }

        public static string FindGameFolder(string str) {
            string[] disks = new string[] {@"C:\", @"D:\", @"E:\", @"F:\"};
            string[] roots = new string[] {"Games", "Program files", "Program files (x86)", ""};
            string[] folders = new string[] {@"Steam\SteamApps\common", @"GoG Galaxy\Games", ""};
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                disks = new string[] {Environment.GetEnvironmentVariable("HOME")};
                roots = new string[] {"Library/Application Support", ".steam"};
                folders = new string[] {"Steam/SteamApps/common", "steam/steamapps/common", "Steam/steamapps/common"};
            }

            foreach (var disk in disks) {
                foreach (var root in roots) {
                    foreach (var folder in folders) {
                        var path = Path.Combine(disk, root);
                        path = Path.Combine(path, folder);
                        path = Path.Combine(path, str);
                        if (Directory.Exists(path)) {
                            if (IsMacPlatform()) {
                                foreach (var dir in Directory.GetDirectories(path)) {
                                    if (dir.EndsWith(".app")) {
                                        path = Path.Combine(path, dir);
                                        break;
                                    }
                                }
                            }

                            return path;
                        }
                    }
                }
            }

            return null;
        }

        public static bool IsMacPlatform() {
            int p = (int) Environment.OSVersion.Platform;
            return (p == 6);
        }
    }
}