using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace SatelliteLoader {
    public class GlobalConfig {
        public static GlobalConfig Instance { get; private set; }
        public Dictionary<string, bool> Enabled;

        public static void Load(string path) {
            if (File.Exists(path)) {
                try {
                    using var reader = new StreamReader(path);
                    Instance = new Deserializer().Deserialize<GlobalConfig>(reader.ReadToEnd());
                    return;
                } catch (Exception e) {
                    SatelliteStarter.Log($"Cannot load LoaderInfo from path {path}");
                    SatelliteStarter.Log(e);
                }
            }

            Instance = new GlobalConfig();
            Save(path);
        }

        public static void Save(string path) {
            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            using var writer = new StreamWriter(path);
            writer.Write(new Serializer().Serialize(Instance));
        }
    }
}