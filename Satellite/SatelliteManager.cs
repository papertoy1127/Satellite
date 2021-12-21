using System;
using System.Reflection;
using Newgrounds;
using UnityEngine;
using UnityEngine.UI;

namespace SatelliteLoader {
    internal class SatelliteManager : MonoBehaviour {
        public static Image Image;
        public static Image Image2;
        private static bool _isGUIOpen;
        public static GUIStyle TitleStyle;
        public static GUIStyle Mainstyle;
        public static GUIStyle Button;
        public static GUIStyle ButtonSelected;
        private static string _version;
        public static bool IsGUIOpen {
            get => _isGUIOpen;
            set {
                Image.gameObject.SetActive(value);
                Image2.gameObject.SetActive(value);
                _isGUIOpen = value;
                if (value) return;
                foreach (var satellite in Satellite._loadedSatellites) {
                    satellite.OnSave();
                }
            }
        }

        private void Awake() {
            DontDestroyOnLoad(gameObject);
            var canvas = gameObject.AddComponent<Canvas>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -100000;

            Image = new GameObject().AddComponent<Image>();
            Image.sprite = Assets.Border;
            Image.color = new Color(0, 0, 0, 0.8f);
            Image.type = Image.Type.Sliced;
            Image.pixelsPerUnitMultiplier = 32;
            Image.fillCenter = true;
            Image.transform.parent = transform;
            var rct = Image.GetComponent<RectTransform>();
            rct.anchorMin = new Vector2(0.5f, 0.5f);
            rct.anchorMax = new Vector2(0.5f, 0.5f);
            rct.pivot = new Vector2(0.5f, 0.5f);
            rct.anchoredPosition = Vector2.zero;
            rct.sizeDelta = new Vector2(960, 540);
            
            Image2 = new GameObject().AddComponent<Image>();
            Image2.sprite = Assets.Border;
            Image2.color = new Color(1, 1, 1);
            Image2.type = Image.Type.Sliced;
            Image2.pixelsPerUnitMultiplier = 32;
            Image2.fillCenter = false;
            Image2.transform.parent = transform;
            var rct2 = Image2.GetComponent<RectTransform>();
            rct2.anchorMin = new Vector2(0.5f, 0.5f);
            rct2.anchorMax = new Vector2(0.5f, 0.5f);
            rct2.pivot = new Vector2(0.5f, 0.5f);
            rct2.anchoredPosition = Vector2.zero;
            rct2.sizeDelta = new Vector2(960, 540);

            TitleStyle = new GUIStyle() {
                font = Assets.NanumBarunGothicBold,
                alignment = TextAnchor.UpperCenter,
                normal = {
                    textColor = Color.white
                }
            };        
            Mainstyle = new GUIStyle() {
                font = Assets.NanumBarunGothicBold,
                alignment = TextAnchor.MiddleLeft,
                normal = {
                    textColor = Color.white
                }
            };
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                
            IsGUIOpen = false;
            Button = new GUIStyle() {
                font = Assets.NanumBarunGothicBold,
                normal = {
                    background = Assets.ButtonDisabled,
                },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            };
            ButtonSelected = new GUIStyle() {
                font = Assets.NanumBarunGothicBold,
                normal = {
                    background = Assets.ButtonEnabled,
                },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            };
        }

        private void Update() {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F11)) {
                IsGUIOpen = !IsGUIOpen;
            }
        }

        private Vector2 _scrollpos = new Vector2(0, 0);
        private Vector2 _scrollpos2 = new Vector2(0, 0);
        private Satellite _selected;

        private void OnGUI() {
            if (!IsGUIOpen) return;
            var rect = new Rect(0, 0, 920, 500) {
                center = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)
            };
            var font = GUI.skin.button.font;
            var fontsize = GUI.skin.button.fontSize;
            GUI.skin.font = Assets.NanumBarunGothic;
            using (new GUIExtended.GUIArea(rect, TitleStyle)) {
                GUILayout.Label($"<size=24>Satellite v{_version}</size>", TitleStyle, GUILayout.Height(36));
                GUILayout.BeginHorizontal();
                using (new GUIExtended.ScrollArea(ref _scrollpos, false, true, GUILayout.Width(300), GUILayout.Height(464))) {
                    GUILayout.BeginVertical();
                    foreach (var satellite in Satellite._loadedSatellites) {
                        GUILayout.BeginHorizontal();
                        if (satellite == _selected) {
                            GUILayout.Button(satellite.Info.ModName, ButtonSelected, GUILayout.Height(40), GUILayout.Width(200));
                        } else {
                            if (GUILayout.Button(satellite.Info.ModName, Button, GUILayout.Height(40), GUILayout.Width(200))) {
                                _selected = satellite;
                            }
                        }
                        GUILayout.Space(10);
                        satellite.Enabled = GUILayout.Toggle(satellite.Enabled, "<size=28><color=#ffffff>" + (satellite.Enabled ? "<color=#00ff00>●</color> On" : "<color=#bbbbbb>○</color> Off") + "</color></size>", Mainstyle, GUILayout.Height(40));
                        GUILayout.EndVertical();
                        GUILayout.Space(5);
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.Space(10);
                using (new GUIExtended.ScrollArea(ref _scrollpos2, false, true, GUILayout.Width(610), GUILayout.Height(464))) {
                    GUILayout.BeginVertical();
                    try {
                        if (_selected?.Enabled == true) _selected.OnGUI();
                    } catch (Exception e) {
                        GUILayout.Label("An error occured");
                        GUILayout.TextArea(e.ToString());
                    }

                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            GUI.skin.font = font;
        }

        private void OnApplicationQuit() {
            foreach (var satellite in Satellite._loadedSatellites) {
                satellite.OnExit();
                satellite.OnSave();
            }
        }
    }
}