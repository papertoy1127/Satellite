using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Unix.Native;
using Sirenix.Utilities;

namespace SatelliteLoader {
    public abstract class Satellite {
        public static bool IsGUIOpen => SatelliteManager.IsGUIOpen;
        public SatelliteInfo Info { get; internal set; }
        public Harmony Harmony => _harmony ??= new Harmony(Info.SatelliteID);
        private Harmony _harmony;
        private bool? _enabled;

        public bool Enabled {
            get => _enabled!.Value;
            set {
                if (_enabled == value) return;
                _enabled = value;
                if (value) OnEnable();
                else OnDisable();
                GlobalConfig.Instance.Enabled[Info.SatelliteID] = value;
                GlobalConfig.Save("Satellites/globalconfig.yml");
            }
        }

        public static ImmutableList<Satellite> LoadedSatellites => _loadedSatellites.ToImmutableList();
        internal static List<Satellite> _loadedSatellites = new List<Satellite>();
        internal static Dictionary<Assembly, Satellite> _satellites = new Dictionary<Assembly, Satellite>();
        public static T Instance<T>() where T : Satellite, new() {
            if (_satellites.TryGetValue(typeof(T).Assembly, out var result)) return (T) result;
            result = new T();
            _satellites[typeof(T).Assembly] = result;
            return (T) result;
        }
        
        public static Satellite Instance(Type type) {
            if (_satellites.TryGetValue(type.Assembly, out var result)) return result;
            result = (Satellite) Activator.CreateInstance(type);
            _satellites[type.Assembly] = result;
            return result;
        }

        public virtual void OnLoad() { }
        public virtual void OnLateLoad() { }
        public virtual void OnExit() { }
        public virtual void OnGUI() { }
        public virtual void OnSave() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public static void WithLoaded(string satelliteId, Action<Satellite> onloaded, Action notloaded = null) {
            var satellite = LoadedSatellites.FirstOrDefault(s => s.Info.SatelliteID == satelliteId);
            if (satellite != null) onloaded?.Invoke(satellite);
            else notloaded?.Invoke();
        }
        
        public static void WithLoaded(string satelliteId, Action onloaded, Action notloaded = null) {
            var satellite = LoadedSatellites.FirstOrDefault(s => s.Info.SatelliteID == satelliteId);
            if (satellite != null) onloaded?.Invoke();
            else notloaded?.Invoke();
        }
    }
}