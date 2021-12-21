using System;
using UnityEngine;

namespace SatelliteLoader {
    public class GUIExtended {
        public class GUIArea : IDisposable {
            public GUIArea(Rect rect, GUIContent content = null) {
                GUILayout.BeginArea(rect, content);
            }
            
            public GUIArea(Rect rect, GUIStyle style) {
                GUILayout.BeginArea(rect, style);
            }

            public void Dispose() {
                GUILayout.EndArea();
            }
        }
        public class RelativeArea : IDisposable {
            public bool Horiozntal;

            public RelativeArea(bool horizontal = false, params GUILayoutOption[] options) {
                Horiozntal = horizontal;
                if (horizontal) GUILayout.BeginHorizontal(options);
                else GUILayout.BeginVertical(options);
            }

            public RelativeArea(bool horizontal, GUIStyle style, params GUILayoutOption[] options) {
                Horiozntal = horizontal;
                if (horizontal) GUILayout.BeginHorizontal(style, options);
                else GUILayout.BeginVertical(style, options);
            }

            public RelativeArea(bool horizontal, GUIStyle style, GUIContent content, params GUILayoutOption[] options) {
                Horiozntal = horizontal;
                if (horizontal) GUILayout.BeginHorizontal(content, style, options);
                else GUILayout.BeginVertical(content, style, options);
            }

            public void Dispose() {
                if (Horiozntal) GUILayout.EndHorizontal();
                else GUILayout.EndVertical();
            }
        }
        
        
        public class ScrollArea : IDisposable {
            public ScrollArea(ref Vector2 vector2, bool horiozntal = true, bool vertical = true, params GUILayoutOption[] options) {
                vector2 = horiozntal
                    ? vertical ? GUILayout.BeginScrollView(vector2, options) :
                    GUILayout.BeginScrollView(vector2, GUI.skin.horizontalScrollbar, null, options)
                    : GUILayout.BeginScrollView(vector2, null, vertical ? GUI.skin.verticalScrollbar : null, options);
            }

            public void Dispose() {
                GUILayout.EndScrollView();
            }
        }
    }
}