using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEngine;
using YamlDotNet.Serialization;
using Debug = UnityEngine.Debug;

namespace SatelliteLoader {
    public class SatelliteInjector {
        public static void Start() {
            Debug.Log("Initializing Satellite...");

            var assembly = Assembly.LoadFrom(Path.Combine("A Dance of Fire and Ice_Data", "Managed", "Satellite", "Satellite.dll"));
            var type = assembly.GetType("SatelliteLoader.SatelliteStarter");
            var method = type.GetMethod("Initialize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }
    }
    
    internal class SatelliteStarter {
        [HarmonyPatch(typeof(ADOStartup), "Startup")]
        private static class StartupPatch {
            private static bool _startup;
            
            public static void Prefix() {
                if (_startup) return;
                _startup = true;
                Assets.Load();
                GlobalConfig.Load("Satellites/globalconfig.yml");
                GlobalConfig.Instance.Enabled ??= new Dictionary<string, bool>();
                new GameObject().AddComponent<SatelliteManager>();
                LoadSatellites();
            }
        }
        
        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        public static class NoInputPatch {
            public static void Postfix(ref bool __result) {
                __result = __result && !SatelliteManager.IsGUIOpen;
            }
        }
        
        private static void Initialize() {
            new Harmony(typeof(SatelliteStarter).GetHashCode().ToString()).PatchAll(Assembly.GetExecutingAssembly());
            SatelliteLogger.LogAction += Debug.Log;
        }

        internal static void Log(object log) {
            SatelliteLogger.Log($"[Satellite] {log}", null);
        }

        private static void LoadSatellites() {
            const string path = "Satellites";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Log($"Loading Satellites from {Path.GetFullPath(path)}");
            var list = new List<SatelliteInfo>();
            foreach (string satdir in Directory.GetDirectories(path)) {
                string infopath = Path.Combine(satdir, "info.yml");
                if (!File.Exists(infopath)) {
                    Log($"info.yml not found on path {Path.GetFullPath(satdir)}");
                    continue;
                }
                try {
                    using var reader = new StreamReader(infopath);
                    var info = new Deserializer().Deserialize<SatelliteInfo>(reader.ReadToEnd());
                    info.Path = satdir;
                    info.ModName = info.SatelliteID;
                    info.RequiredSatellites ??= new string[] { };
                    list.Add(info);
                } catch (Exception e) {
                    Log($"Failed to load a Satellite from {Path.GetFullPath(satdir)}");
                    Log(e);
                }
            }

            var copy = list.ToList();
            list = new List<SatelliteInfo>();
            var unloaded = new List<SatelliteInfo>();

            bool LoadSatellite(SatelliteInfo info) {
                if (list.Contains(info)) return true;
                if (info == null || unloaded.Contains(info)) return false;
                if (info.RequiredSatellites.All(satellite => LoadSatellite(copy.Find(s => s.SatelliteID == satellite)))) {
                    list.Add(info);
                    return true;
                }
                unloaded.Add(info);
                return false;
            }
            foreach (var info in copy) {
                LoadSatellite(info);
            }

            foreach (var info in list.ToList()) {
                if (unloaded.Contains(info)) continue;
                var assemblies = new List<Assembly>();
                
                string[] files = Directory.GetFiles(info.Path);
                foreach (string dll in files.Where(s => s.EndsWith(".dll"))) {
                    try {
                        var assembly = Assembly.LoadFrom(dll);
                        assemblies.Add(assembly);
                    } catch (Exception e) {
                        Log($"Failed to load assembly from file {dll}");
                        Log(e.ToString());
                        foreach (var fail in list.Where(s => s.SatelliteID == info.SatelliteID || s.RequiredSatellites.Contains(s.SatelliteID))) {
                            list.Remove(fail);
                            unloaded.Add(fail);
                        }

                        goto loopend;
                    }
                }

                var entry =
                    assemblies
                        .SelectMany(a => a.GetSafeTypes())
                        .Where(t => t.IsSubclassOf(typeof(Satellite)))
                        .FirstOrDefault(m => m.FullName == info.Entry);

                if (entry == null) {
                    Log($"Entry not found from {info.Path}");
                    continue;
                }

                var instance = Satellite.Instance(entry);
                instance.Info = info;
                if (GlobalConfig.Instance.Enabled.TryGetValue(info.SatelliteID, out var value)) {
                    instance.Enabled = value;
                } else {
                    instance.Enabled = true;
                }
                Satellite._loadedSatellites.Add(instance);

                if (instance.Enabled) {
                    try {
                        instance.OnLoad();
                        Log($"Loaded Satellite {info.SatelliteID}");
                    } catch (Exception e) {
                        Log($"Loading Satellite {info.SatelliteID} threw an exception.");
                        Log(e);
                    }
                } else {
                    Log($"Skipping Satellite {info.SatelliteID} (Disabled)");
                }
                
                loopend: ;
            }

            foreach (var satellite in Satellite._loadedSatellites.Where(s => s.Enabled)) {
                try {
                    satellite.OnLateLoad();
                } catch { }
            }
            
            Log($"Loaded {list.Count} Satellites.");
            if (unloaded.Count > 0) {
                Log($"Failed to load {unloaded.Count} Satellites.");
            }
        }
    }
}