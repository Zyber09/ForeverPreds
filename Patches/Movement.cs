using GorillaLocomotion.Climbing;
using HarmonyLib;
using UnityEngine;

namespace ForeverPreds.Patches
{
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "LateUpdate")]
    internal class ExamplePatch
    {
        public static void Prefix(GorillaLocomotion.GTPlayer __instance)
        {
            if (lvT == null || rvT == null)
                CreateVelocityTrackers();

            if (ControllerInputPoller.instance.rightGrab && Plugin.instance.joystickDisable)
                ControllerInputPoller.instance.rightControllerGripFloat = Plugin.instance.GetJoystickDown() ? 1f : 0f;

            if (!(ControllerInputPoller.instance.rightGrab && Plugin.instance.joystickDisable))
            VelocityLongArms(Plugin.instance.prediction);
        }

        public static GameObject lvT = null;
        public static GameObject rvT = null;
        public static void CreateVelocityTrackers()
        {
            lvT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(lvT.GetComponent<BoxCollider>());
            Object.Destroy(lvT.GetComponent<Rigidbody>());
            lvT.GetComponent<Renderer>().enabled = false;
            lvT.AddComponent<GorillaVelocityTracker>();

            rvT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(rvT.GetComponent<BoxCollider>());
            Object.Destroy(rvT.GetComponent<Rigidbody>());
            rvT.GetComponent<Renderer>().enabled = false;
            rvT.AddComponent<GorillaVelocityTracker>();
        }

        public static void DestroyVelocityTrackers()
        {
            Debug.Log(lvT);
            Debug.Log(rvT);
        }

        public static void VelocityLongArms(float power)
        {
            lvT.transform.position = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.leftHandTransform.position;
            rvT.transform.position = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.rightHandTransform.position;
            GorillaLocomotion.GTPlayer.Instance.leftControllerTransform.transform.position -= lvT.GetComponent<GorillaVelocityTracker>().GetAverageVelocity(true, 0) * (power / 333f);
            GorillaLocomotion.GTPlayer.Instance.rightControllerTransform.transform.position -= rvT.GetComponent<GorillaVelocityTracker>().GetAverageVelocity(true, 0) * (power / 333f);
        }
    }
}
