using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SatelliteLoader {
    internal static class Assets {
        public static AssetBundle AssetBundle { get; private set; }
        public static Sprite Border { get; private set; }
        public static Texture2D ButtonDisabled { get; private set; }
        public static Texture2D ButtonEnabled { get; private set; }
        public static Font NanumBarunGothic { get; private set; }
        public static Font NanumBarunGothicBold { get; private set; }
        
        internal static void Load() {
            AssetBundle = UnityEngine.AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Satellite.satellite"));
            Border = AssetBundle.LoadAsset<Sprite>("border");
            ButtonDisabled = AssetBundle.LoadAsset<Texture2D>("buttondisabled");
            ButtonEnabled = AssetBundle.LoadAsset<Texture2D>("buttonenabled");
            NanumBarunGothic = AssetBundle.LoadAsset<Font>("NanumBarunGothic");
            NanumBarunGothicBold = AssetBundle.LoadAsset<Font>("NanumBarunGothicBold");
            
            Console.WriteLine(string.Join(", ", AssetBundle.GetAllAssetNames()));
            Console.WriteLine($"Border is {Border}");
        }

        private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight) {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float) targetWidth);
            float incY = (1.0f / (float) targetHeight);
            for (int px = 0; px < rpixels.Length; px++) {
                rpixels[px] = source.GetPixelBilinear(incX * ((float) px % targetWidth),
                    incY * ((float) Mathf.Floor(px / targetWidth)));
            }

            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }
    }
}