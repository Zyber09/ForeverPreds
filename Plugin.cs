using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;

namespace ForeverPreds
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// What i added to ForeverPreds:
        ///     Better Looking GUI
        ///     Saving  (uses bepinex config)
        ///     Loading (uses bepinex config)
        ///         
        ///not really a big change but yeah
        /// </summary>


        public static Plugin instance;
        public float prediction; 
        public bool joystickDisable; 

        //config entries
        private static ConfigEntry<float> predictionConfig;
        private static ConfigEntry<bool> joystickDisableConfig;

        //window vars
        private Rect windowRect = new Rect(20, 20, 380, 260);
        private GUISkin Style;
        private bool showWindow = true;

        //bools
        private bool styleInit = false;
        private bool sliderHover = false;

        //color list
        private readonly Color bgColor = new Color(0.1f, 0.1f, 0.12f, 0.98f);
        private readonly Color shadowColor = new Color(0, 0, 0, 0.4f);
        private readonly Color primaryColor = new Color(0.98f, 0.6f, 0.75f, 1f);
        private readonly Color hoverColor = new Color(1f, 0.7f, 0.82f, 1f);
        private readonly Color clickColor = new Color(0.9f, 0.5f, 0.65f, 1f);
        private readonly Color colorThing = new Color(0.35f, 0.2f, 0.25f, 1f);

        //Textures
        private Texture2D windowBgTex, shadowBgTex, buttonNormalTex, buttonHoverTex, buttonClickTex;
        private Texture2D toggleBgNormalTex, toggleBgHoverTex, toggleOnNormalTex, toggleOnHoverTex;
        private Texture2D sliderTrackTex, sliderFillNormalTex, sliderFillHoverTex;

        void Start()
        {
            instance = this;
            predictionConfig = Config.Bind("General", "Prediction Power", 4f, "How much to predict hand movement."); 
            joystickDisableConfig = Config.Bind("General", "Right Joystick Disable", false, "If enabled, holding the right joystick click will disable the predictions.");

            LoadSettings();
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void Update()
        {
            if (Keyboard.current.insertKey.wasPressedThisFrame)
            {
                showWindow = !showWindow;
            }
        }

        void OnGUI()
        {
            if (!styleInit)
            {
                InitStyle();
                styleInit = true;
            }

            if (!showWindow) return;

            GUI.skin = Style;

            Rect Rect = new Rect(windowRect.x + 4, windowRect.y + 4, windowRect.width, windowRect.height);
            GUI.Box(Rect, GUIContent.none, "shadow");

            windowRect = GUI.Window(0, windowRect, CreateGUI, "");
        }

        void CreateGUI(int id)
        {
            GUI.Label(new Rect(0, 0, windowRect.width, 35), $"{PluginInfo.Name} (Insert to toggle)", "title");
            GUILayout.BeginArea(new Rect(15, 45, windowRect.width - 30, windowRect.height - 60));
            GUILayout.BeginVertical();
            GUILayout.Label($"Prediction Power: {prediction:F2}");
            GUILayout.Space(5);
            prediction = THICK_SliderHehe(prediction, 0f, 150f);
            GUILayout.Space(15);
            joystickDisable = GUILayout.Toggle(joystickDisable, " Right Joystick to Disable");
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) SaveSettings();
            GUILayout.Space(10);
            if (GUILayout.Button("Load")) LoadSettings();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<i>Prediction slider by @goldentrophy</i>", "smallLabel");
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUI.DragWindow(new Rect(0, 0, 10000, 35));
        }

        private float THICK_SliderHehe(float value, float min, float max)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(24));
            Event e = Event.current;
            sliderHover = position.Contains(e.mousePosition);
            GUIStyle trackStyle = GUI.skin.FindStyle("sliderTrack");
            GUIStyle fillStyle = GUI.skin.FindStyle("sliderFill");
            fillStyle.normal.background = sliderHover ? sliderFillHoverTex : sliderFillNormalTex;
            GUI.Box(position, GUIContent.none, trackStyle);
            float progress = Mathf.InverseLerp(min, max, value);
            GUI.BeginGroup(new Rect(position.x, position.y, position.width * progress, position.height));
            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, fillStyle);
            GUI.EndGroup();
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && sliderHover)
            {
                float newProgress = (e.mousePosition.x - position.x) / position.width;
                value = Mathf.Lerp(min, max, newProgress);
                value = Mathf.Clamp(value, min, max);
                e.Use();
            }
            return value;
        }

        void SaveSettings()
        {
            predictionConfig.Value = prediction;
            joystickDisableConfig.Value = joystickDisable;
            Config.Save();
        }

        void LoadSettings()
        {
            prediction = predictionConfig.Value;
            joystickDisable = joystickDisableConfig.Value;
        }

      
        private bool IsSteam = true;
        private bool hasSteamChecked;
        public bool GetJoystickDown()
        {
            if (!hasSteamChecked)
            {
                IsSteam = Traverse.Create(PlayFabAuthenticator.instance).Field("platform").GetValue().ToString().ToLower() == "steam";
                hasSteamChecked = true;
            }
            bool rightJoystickClick = false;
            if (IsSteam)
                rightJoystickClick = SteamVR_Actions.gorillaTag_RightJoystickClick.GetState(SteamVR_Input_Sources.RightHand);
            else
                ControllerInputPoller.instance.rightControllerDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out rightJoystickClick);
            return rightJoystickClick;
        }



        private Texture2D MakeRoundedTex(int width, int height, Color c, float r)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < r && y < r){ if (Vector2.Distance(new Vector2(x, y), new Vector2(r, r)) > r) pixels[y * width + x] = Color.clear; else pixels[y * width + x] = c; }
                    else if (x > width - r && y < r) { if (Vector2.Distance(new Vector2(x, y), new Vector2(width - r, r)) > r) pixels[y * width + x] = Color.clear; else pixels[y * width + x] = c; }
                    else if (x < r && y > height - r) { if (Vector2.Distance(new Vector2(x, y), new Vector2(r, height - r)) > r) pixels[y * width + x] = Color.clear; else pixels[y * width + x] = c; }
                    else if (x > width - r && y > height - r) { if (Vector2.Distance(new Vector2(x, y), new Vector2(width - r, height - r)) > r) pixels[y * width + x] = Color.clear; else pixels[y * width + x] = c; }
                    else { pixels[y * width + x] = c; }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void InitStyle()
        {
            int buttonR = 8;
            int toggleR = 4;
            int sliderR = 6;

            windowBgTex = MakeRoundedTex(64, 64, bgColor, buttonR);
            shadowBgTex = MakeRoundedTex(64, 64, shadowColor, buttonR + 1);
            buttonNormalTex = MakeRoundedTex(64, 64, colorThing, buttonR);
            buttonHoverTex = MakeRoundedTex(64, 64, primaryColor, buttonR);
            buttonClickTex = MakeRoundedTex(64, 64, clickColor, buttonR);
            toggleBgNormalTex = MakeRoundedTex(16, 16, colorThing, toggleR);
            toggleBgHoverTex = MakeRoundedTex(16, 16, primaryColor, toggleR);
            toggleOnNormalTex = MakeRoundedTex(16, 16, primaryColor, toggleR);
            toggleOnHoverTex = MakeRoundedTex(16, 16, hoverColor, toggleR);
            sliderTrackTex = MakeRoundedTex(64, 20, colorThing, sliderR);
            sliderFillNormalTex = MakeRoundedTex(64, 20, primaryColor, sliderR);
            sliderFillHoverTex = MakeRoundedTex(64, 20, hoverColor, sliderR);
            Style = ScriptableObject.CreateInstance<GUISkin>();
            Style.window = new GUIStyle { normal = { background = windowBgTex }, border = new RectOffset(buttonR, buttonR, buttonR, buttonR) };
            Style.label = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white }, alignment = TextAnchor.MiddleLeft };
            Style.customStyles = new GUIStyle[]
            {
                new GUIStyle { name = "shadow", normal = { background = shadowBgTex }, border = new RectOffset(buttonR + 1, buttonR + 1, buttonR + 1, buttonR + 1) },
                new GUIStyle(Style.label) { name = "title", alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold },
                new GUIStyle(Style.label) { name = "smallLabel", normal = { textColor = Color.gray }, alignment = TextAnchor.MiddleCenter, richText = true }
            };
            Style.button = new GUIStyle
            {
                normal = { background = buttonNormalTex, textColor = Color.white },
                hover = { background = buttonHoverTex, textColor = Color.white },
                active = { background = buttonClickTex, textColor = Color.white },
                border = new RectOffset(buttonR, buttonR, buttonR, buttonR),
                padding = new RectOffset(0, 0, 8, 8),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            Style.toggle = new GUIStyle
            {
                normal = { textColor = Color.white },
                hover = { textColor = hoverColor },
                padding = new RectOffset(22, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft,
                fixedHeight = 20,
                border = new RectOffset(toggleR, toggleR, toggleR, toggleR)
            };
            Style.toggle.normal.background = toggleBgNormalTex;
            Style.toggle.hover.background = toggleBgHoverTex;
            Style.toggle.onNormal.background = toggleOnNormalTex;
            Style.toggle.onHover.background = toggleOnHoverTex;
            var sliderTrack = new GUIStyle
            {
                name = "sliderTrack",
                normal = { background = sliderTrackTex },
                border = new RectOffset(sliderR, sliderR, sliderR, sliderR)
            };
            var sliderFill = new GUIStyle
            {
                name = "sliderFill",
                normal = { background = sliderFillNormalTex },
                border = new RectOffset(sliderR, sliderR, sliderR, sliderR)
            };
            Style.customStyles = AddStyle(Style.customStyles, sliderTrack);
            Style.customStyles = AddStyle(Style.customStyles, sliderFill);
        }

        private GUIStyle[] AddStyle(GUIStyle[] original, GUIStyle toAdd)
        {
            var list = new System.Collections.Generic.List<GUIStyle>(original);
            list.Add(toAdd);
            return list.ToArray();
        }

    }
}