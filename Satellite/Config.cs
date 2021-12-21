using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

namespace SatelliteLoader {
    public abstract class Config {
        private static readonly Dictionary<Type, Config> _configs = new Dictionary<Type, Config>();

        public static readonly List<IYamlTypeConverter> Converters = new List<IYamlTypeConverter> {
            new ColorYamlTypeConverter(),
        };

        public static T Instance<T>(Satellite satellite) where T : Config, new() {
            if (_configs.ContainsKey(typeof(T))) return (T) _configs[typeof(T)];
            var settingpath = Path.Combine(satellite.Info.Path, "config.yml");
            if (File.Exists(settingpath)) {
                try {
                    using var reader = new StreamReader(settingpath);
                    var builder = new DeserializerBuilder();
                    foreach (var converter in Converters) {
                        builder.WithTypeConverter(converter);
                    }
                    var config = builder.Build().Deserialize<T>(reader.ReadToEnd());
                    _configs[typeof(T)] = config;
                    return config;
                } catch (Exception e) {
                    SatelliteStarter.Log($"Cannot load config of type {typeof(T).Name}");
                    SatelliteStarter.Log(e);
                }
            }

            _configs[typeof(T)] = new T();
            return (T) _configs[typeof(T)];
        }

        public virtual void Save(string path) {
            path = Path.Combine(path, "config.yml");
            var builder = new SerializerBuilder();
            foreach (var c in Converters) {
                builder.WithTypeConverter(c);
            }

            using var writer = new StreamWriter(path);
            writer.Write(builder.Build().Serialize(this));
        }
    }
}