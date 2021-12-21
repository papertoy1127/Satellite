using System;
using System.Reflection;

namespace SatelliteLoader {
    public class SatelliteLogger {
        public static Action<object> LogAction { get; set; }

        public static void Log(object log, string prefix) {
            LogAction($"{prefix}{log}");
        }
        
        public static void Log(object log) {
            var prefix = Satellite._satellites[Assembly.GetCallingAssembly()].Info.ModName;
            LogAction($"[{prefix}] {log}");
        }
    }
}