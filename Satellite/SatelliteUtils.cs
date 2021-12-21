using System;
using System.Linq;
using System.Reflection;

namespace SatelliteLoader {
    public static class SatelliteUtils {
        public static Type[] GetSafeTypes(this Assembly assembly) {
            Type[] types;
            try {
                types = assembly.GetTypes();
            } catch (ReflectionTypeLoadException e) {
                types = e.Types;
            }

            types = types.Where(t => t != null).ToArray();

            return types;
        }
    }
}