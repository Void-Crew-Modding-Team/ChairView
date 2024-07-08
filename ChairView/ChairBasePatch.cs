using HarmonyLib;
using Photon.Pun;
using System;
using CG.Game;
using CG.Ship.Modules;
using System.Reflection;

namespace ChairView
{
    [HarmonyPatch(typeof(ChairBase))]
    internal class ChairBasePatch
    {
        private static ChairBase playerChair;
        internal static bool isHelm = true;

        private static readonly MethodInfo AbilityStartedMethod = AccessTools.Method(typeof(ControllingHelm), "AbilityStarted");
        private static readonly MethodInfo AbilityStoppedMethod = AccessTools.Method(typeof(ControllingHelm), "AbilityStopped");

        [HarmonyPostfix]
        [HarmonyPatch("TakeChairLocal")]
        static void TakeChairLocal(ChairBase __instance)
        {
            if (__instance.photonView.Owner != PhotonNetwork.LocalPlayer || __instance is TurretChair || __instance is not TakeoverChair ||
                __instance == ClientGame.Current.PlayerShip.gameObject.GetComponentInChildren<Helm>().Chair) return;

            playerChair = __instance;
            ControllingHelmPatch.controllingHelm.AbilityActivator = ClientGame.Current.PlayerShip.gameObject.GetComponentInChildren<Helm>();
            isHelm = false;
            AbilityStartedMethod.Invoke(ControllingHelmPatch.controllingHelm, null);
        }

        [HarmonyPrefix]
        [HarmonyPatch("FreeChairLocal")]
        static void FreeChairLocal(ChairBase __instance)
        {
            if (__instance != playerChair) return;

            playerChair = null;
            AbilityStoppedMethod.Invoke(ControllingHelmPatch.controllingHelm, new object[] { true });
            isHelm = true;
        }

        internal static void Reset(object sender = null, EventArgs e = null)
        {
            if (ControllingHelmPatch.controllingHelm == null) return;

            AbilityStoppedMethod.Invoke(ControllingHelmPatch.controllingHelm, new object[] { true });
            isHelm = true;
            ControllingHelmPatch.controllingHelm = null;
        }
    }
}
