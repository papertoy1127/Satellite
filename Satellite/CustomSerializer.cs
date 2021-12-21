using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SatelliteLoader {
    public sealed class ColorYamlTypeConverter : IYamlTypeConverter {
        public bool Accepts(Type type) {
            return type == typeof(Color) || type == typeof(Color32);
        }

        public object ReadYaml(IParser parser, Type type) {
            var result = ParseHtmlString(GetScalarValue(parser));
            if (type == typeof(Color32)) return result;
            parser.MoveNext();
            return (Color) result;
        }

        private static Color32 ParseHtmlString(string color) {
            try {
                if (color.Length == 6) {
                    var r = color.Substring(0, 2);
                    var g = color.Substring(2, 2);
                    var b = color.Substring(4, 2);
                    return new Color32(Convert.ToByte(r, 16), Convert.ToByte(g, 16), Convert.ToByte(b, 16), 255);
                } else if (color.Length == 8) {
                    var r = color.Substring(0, 2);
                    var g = color.Substring(2, 2);
                    var b = color.Substring(4, 2);
                    var a = color.Substring(6, 2);
                    return new Color32(Convert.ToByte(r, 16), Convert.ToByte(g, 16), Convert.ToByte(b, 16),
                        Convert.ToByte(a, 16));
                }
            } catch { } 
            throw new InvalidDataException("Value is not a valid hex string.");
        }

        private string GetScalarValue(IParser parser) {
            Scalar scalar;

            scalar = parser.Current as Scalar;

            if (scalar == null) {
                throw new InvalidDataException("Failed to retrieve scalar value.");
            }

            // You could replace the above null check with parser.Expect<Scalar> which will throw its own exception

            return scalar.Value;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type) {
            Color32 v;
            if (value is Color c1) {
                v = c1;
            } else if (value is Color32 c2) {
                v = c2;
            } else return;

            emitter.Emit(new Scalar(null,
                v.a == 255 ? ColorUtility.ToHtmlStringRGB(v) : ColorUtility.ToHtmlStringRGBA(v)));
        }
    }
}