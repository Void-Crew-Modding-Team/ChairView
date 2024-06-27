using CG;
using CG.Client;
using CG.Game.Player;
using CG.Input;
using CG.Ship.Modules;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.InputSystem;
using VoidManager.Utilities;

namespace ChairView
{
    [HarmonyPatch(typeof(ControllingHelm))]
    internal class ControllingHelmPatch
    {
        internal static ControllingHelm controllingHelm;
        private static List<ControllingHelm> potentialHelm = new();
        private static LocalPlayer nullPlayer;

        private static readonly FieldInfo helmParentSpacecraftField = AccessTools.Field(typeof(ControllingHelm), "_helmParentSpacecraft");
        private static readonly FieldInfo helmField = AccessTools.Field(typeof(ControllingHelm), "_helm");
        private static readonly FieldInfo localPlayerField = AccessTools.Field(typeof(ControllingHelm), "_localPlayer");
        private static readonly MethodInfo ToggleExternalThirdPersonShipViewMethod = AccessTools.Method(typeof(ControllingHelm), "ToggleExternalThirdPersonShipView");

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void Awake(ControllingHelm __instance, LocalPlayer ____localPlayer)
        {
            nullPlayer = ____localPlayer;
            if (potentialHelm.Count == 0)
                VoidManager.Events.Instance.LateUpdate += CheckHelm;
            potentialHelm.Add(__instance);
        }

        private static void CheckHelm(object sender, EventArgs e)
        {
            for (int i = potentialHelm.Count - 1; i >= 0; i--)
            {
                ControllingHelm helm = potentialHelm[i];
                LocalPlayer player = (LocalPlayer)localPlayerField.GetValue(helm);
                if (LocalPlayer.Instance == player)
                {
                    controllingHelm = helm;
                    potentialHelm.RemoveAt(i);
                }
                else
                {
                    potentialHelm.RemoveAt(i);
                }
            }

            if (potentialHelm.Count == 0)
                VoidManager.Events.Instance.LateUpdate -= CheckHelm;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("AbilityStarted")]
        static IEnumerable<CodeInstruction> AbilityStarted(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new()
            {
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Callvirt)
            };

            List<CodeInstruction> patchSequence = new()
            {
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ControllingHelmPatch), nameof(ShipControlEvent)))
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, patchSequence, HarmonyHelpers.PatchMode.REPLACE, HarmonyHelpers.CheckMode.NEVER);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("AbilityStopped")]
        static IEnumerable<CodeInstruction> AbilityStopped(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new()
            {
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Callvirt)
            };

            List<CodeInstruction> patchSequence = new()
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ControllingHelmPatch), nameof(ShipControlEvent)))
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, patchSequence, HarmonyHelpers.PatchMode.REPLACE, HarmonyHelpers.CheckMode.NEVER);
        }

        private static void ShipControlEvent(bool b)
        {
            if (OnChairSeatedChangedPatch.isHelm)
                ViewEventBus.Instance.OnShipControlToggle.Publish((CG.Space.SpaceObjects.ISpaceCraft)helmParentSpacecraftField.GetValue(controllingHelm), b);
        }

        [HarmonyPrefix]
        [HarmonyPatch("EnableInput")]
        static bool EnableInput()
        {
            if (OnChairSeatedChangedPatch.isHelm) return true;

            InputActionReferences inputs = ServiceBase<InputService>.Instance.InputActionReferences;
            Helm helm = (Helm)helmField.GetValue(controllingHelm);
            inputs.ChangeView.action.started += (Action<InputAction.CallbackContext>) ToggleExternalThirdPersonShipViewMethod.CreateDelegate(typeof(Action<InputAction.CallbackContext>), controllingHelm);
            inputs.CameraViewHorizontal.action.performed += helm.SetMouseDeltaX;
            inputs.CameraViewHorizontal.action.canceled += helm.SetMouseDeltaX;
            inputs.CameraViewVertical.action.performed += helm.SetMouseDeltaY;
            inputs.CameraViewVertical.action.canceled += helm.SetMouseDeltaY;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DisableInput")]
        static bool DisableInput()
        {
            if (OnChairSeatedChangedPatch.isHelm) return true;

            InputActionReferences inputs = ServiceBase<InputService>.Instance.InputActionReferences;
            Helm helm = (Helm)helmField.GetValue(controllingHelm);
            inputs.ChangeView.action.started -= (Action<InputAction.CallbackContext>)ToggleExternalThirdPersonShipViewMethod.CreateDelegate(typeof(Action<InputAction.CallbackContext>), controllingHelm);
            inputs.CameraViewHorizontal.action.performed -= helm.SetMouseDeltaX;
            inputs.CameraViewHorizontal.action.canceled -= helm.SetMouseDeltaX;
            inputs.CameraViewVertical.action.performed -= helm.SetMouseDeltaY;
            inputs.CameraViewVertical.action.canceled -= helm.SetMouseDeltaY;
            return false;
        }
    }
}
