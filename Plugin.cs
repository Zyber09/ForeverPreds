using BepInEx;
using GorillaNetworking;
using HarmonyLib;
using System;
using UnityEngine;
using Valve.VR;


namespace ForeverPreds
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public float prediction = 4f;
        public bool joystickDisable;
        void Start()
        {
            instance = this;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnGUI()
        {
            prediction = GUI.HorizontalSlider(new Rect(4f, 4f, 400f, 40f), prediction, 0f, 150f);
            GUI.Label(new Rect(414f, 4f, 1080f, 40f), "Power: " + prediction.ToString());
            GUI.Label(new Rect(4f, 40f, 900f, 40f), "Prediction slider by @goldentrophy");
            joystickDisable = GUI.Toggle(new Rect(300f, 40f, 1080f, 40f), joystickDisable, "Right Joystick to Disable");
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
    }
}
