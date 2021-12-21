using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace SatelliteLoader {
    public static class Injector {
        [Flags]
        public enum Actions {
            Install = 1,
            Restore = 2,
            Delete = 4,
        }

        private static bool CheckApplicationAlreadyRunning(out Process result) {
            result = null;
            var id = Process.GetCurrentProcess().Id;
            var name = Process.GetCurrentProcess().ProcessName;
            foreach (var p in Process.GetProcessesByName(name)) {
                if (p.Id != id) {
                    result = p;
                    return true;
                }
            }

            return false;
        }

        private static readonly List<string> LibraryFiles = new List<string>() {
            "0Harmony.dll",
            "YamlDotNet.dll",
            "dnlib.dll",
            "Satellite.dll",
        };

        private static List<string> _libraryPaths;

        public static string AdofaiPath = @"C:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice";
        private static string _managedPath => Path.Combine(AdofaiPath, @"A Dance of Fire and Ice_Data\Managed");
        private static string _dllName = "UnityEngine.CoreModule.dll";
        private static string _managerPath => Path.Combine(_managedPath, "Satellite");
        private static string _entryPoint = $"[{_dllName}]UnityEngine.Object.cctor:Before";
        private static ModuleDefMD _injectedAssemblyDef;

        static Injector() {
            _libraryPaths = new List<string>();
            foreach (var item in LibraryFiles) {
                _libraryPaths.Add(Path.Combine(_managerPath, item));
            }
        }
        
        public static bool Init(Actions action) {
            var path = Path.Combine(_managedPath, _dllName);
            if (action == Actions.Restore) {
                DoactionLibraries(Actions.Restore);
                var original = path + ".original_";
                if (File.Exists(original)) {
                    try {
                        File.Delete(path);
                        File.Move(original, path);
                        Console.WriteLine("Restore success");
                        return true;
                    } catch {
                        return false;
                    }
                }

                return false;
            }
            var assemblyDef = ModuleDefMD.Load(File.ReadAllBytes(path));
            Console.WriteLine(path);
            Console.WriteLine(assemblyDef.Name);
            _injectedAssemblyDef = assemblyDef;
            return InjectAssembly(action, assemblyDef);
        }
        
        //static string machineConfigPath = null;
        //static XDocument machineDoc = null;
        private static bool InjectAssembly(Actions action, ModuleDefMD assemblyDef, bool write = true) {
            var assemblyPath = Path.Combine(_managedPath, assemblyDef.Name);
            var originalAssemblyPath = $"{assemblyPath}.original_";

            var success = false;

            switch (action) {
                case Actions.Install: {
                    try {
                        Console.WriteLine("=======================================");

                        if (!Directory.Exists(_managerPath))
                            Directory.CreateDirectory(_managerPath);

                        InjectionUtils.MakeBackup(assemblyPath);
                        InjectionUtils.MakeBackup(_libraryPaths);

                        if (!InjectionUtils.IsDirty(assemblyDef)) {
                            File.Copy(assemblyPath, originalAssemblyPath, true);
                            InjectionUtils.MakeDirty(assemblyDef);
                        }

                        if (!InjectAssembly(Actions.Delete, _injectedAssemblyDef, assemblyDef != _injectedAssemblyDef)) {
                            Console.WriteLine("Installation failed. Can't uninstall the previous version.");
                            goto EXIT;
                        }

                        Console.WriteLine($"Applying patch to '{Path.GetFileName(assemblyPath)}'...");

                        if (!InjectionUtils.TryGetEntryPoint(assemblyDef, _entryPoint, out var methodDef, out var insertionPlace,
                            true)) {
                            goto EXIT;
                        }


                        var starterDef = ModuleDefMD.Load(typeof(SatelliteInjector).Module);//(nameof(Satellite) + ".dll");
                        var starter = starterDef.Types.First(x => x.Name == nameof(SatelliteInjector));
                        starterDef.Types.Remove(starter);
                        assemblyDef.Types.Add(starter);
                        Console.WriteLine($"Type is {starter}");
                        Console.WriteLine($"Assembly is {starter.Module}");
                        var instr = OpCodes.Call.ToInstruction(starter.Methods.First(x =>
                            x.Name == nameof(SatelliteInjector.Start)));
                        
                        Console.WriteLine($"Method is {instr}");
                        if (insertionPlace == "before") {
                            methodDef.Body.Instructions.Insert(0, instr);
                        } else {
                            methodDef.Body.Instructions.Insert(methodDef.Body.Instructions.Count - 1, instr);
                        }

                        assemblyDef.Write(assemblyPath);
                        DoactionLibraries(Actions.Install);
                        var satdir = Path.Combine(AdofaiPath, "Satellites");
                        if (Directory.Exists(satdir))
                            Directory.CreateDirectory(satdir);
                        Console.WriteLine("Installation was successful.");

                        success = true;
                    } catch (Exception e) {
                        Console.WriteLine(e.ToString());
                        InjectionUtils.RestoreBackup(assemblyPath);
                        InjectionUtils.RestoreBackup(_libraryPaths);
                        Console.WriteLine("Installation failed.");
                    }
                }
                    break;

                case Actions.Delete: {
                    try {
                        if (write) {
                            Console.WriteLine("=======================================");
                        }

                        var newWayInstalled = assemblyDef.Types.FirstOrDefault(x => x.Name == nameof(SatelliteInjector));

                        if (newWayInstalled != null) {
                            if (write) {
                                InjectionUtils.MakeBackup(assemblyPath);
                                InjectionUtils.MakeBackup(_libraryPaths);
                            }

                            Console.WriteLine("Removing patch...");

                            Instruction instr;

                            instr = OpCodes.Call.ToInstruction(newWayInstalled.Methods.First(x =>
                                x.Name == nameof(SatelliteInjector.Start)));

                            if (!string.IsNullOrEmpty(_entryPoint)) {
                                if (!InjectionUtils.TryGetEntryPoint(assemblyDef, _entryPoint, out var methodDef, out _, true)) {
                                    goto EXIT;
                                }

                                Console.WriteLine($"Remove {methodDef}");

                                for (int i = 0; i < methodDef.Body.Instructions.Count; i++) {
                                    if (methodDef.Body.Instructions[i].OpCode == instr.OpCode &&
                                        methodDef.Body.Instructions[i].Operand == instr.Operand) {
                                        methodDef.Body.Instructions.RemoveAt(i);
                                        break;
                                    }
                                }
                            }

                            assemblyDef.Types.Remove(newWayInstalled);

                            if (!InjectionUtils.IsDirty(assemblyDef)) {
                                InjectionUtils.MakeDirty(assemblyDef);
                            }

                            if (write) {
                                assemblyDef.Write(assemblyPath);
                                DoactionLibraries(Actions.Delete);
                                Console.WriteLine("Removal was successful.");
                            }
                        }

                        success = true;
                    } catch (Exception e) {
                        Console.WriteLine(e.ToString());
                        if (write) {
                            InjectionUtils.RestoreBackup(assemblyPath);
                            InjectionUtils.RestoreBackup(_libraryPaths);
                            Console.WriteLine("Removal failed.");
                        }
                    }
                }
                    break;
            }

            EXIT:

            if (write) {
                try {
                    InjectionUtils.DeleteBackup(assemblyPath);
                    InjectionUtils.DeleteBackup(_libraryPaths);
                } catch (Exception) { }
            }

            return success;
        }

        public static void DoactionLibraries(Actions action) {
            if (action == Actions.Install) {
                Console.WriteLine($"Copying files to game...");
            } else {
                Console.WriteLine($"Deleting files from game...");
            }

            foreach (var destpath in _libraryPaths) {
                var filename = Path.GetFileName(destpath);
                if (action == Actions.Install) {
                    var sourcepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                    if (File.Exists(destpath)) {
                        var source = new FileInfo(sourcepath);
                        var dest = new FileInfo(destpath);
                        if (dest.LastWriteTimeUtc == source.LastWriteTimeUtc)
                            continue;

                        //File.Copy(path, $"{path}.old_", true);
                    }

                    Console.WriteLine($"  {filename}");
                    File.Copy(sourcepath, destpath, true);
                } else {
                    if (File.Exists(destpath)) {
                        Console.WriteLine($"  {filename}");
                        File.Delete(destpath);
                    }
                }
            }
        }

        private static bool RestoreOriginal(string file, string backup) {
            try {
                File.Copy(backup, file, true);
                Console.WriteLine("Original files restored.");
                File.Delete(backup);
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            return false;
        }
    }
}