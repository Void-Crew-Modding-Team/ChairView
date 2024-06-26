using HarmonyLib;
using Photon.Pun;
using System;
using CG.Game;
using CG.Ship.Modules;
using System.Reflection;

namespace ChairView
{
    [HarmonyPatch(typeof(OnChairSeatedChanged))]
    internal class OnChairSeatedChangedPatch
    {
        private static ChairBase playerChair;
        internal static bool isHelm = true;

        private static readonly MethodInfo AbilityStartedMethod = AccessTools.Method(typeof(ControllingHelm), "AbilityStarted");
        private static readonly MethodInfo AbilityStoppedMethod = AccessTools.Method(typeof(ControllingHelm), "AbilityStopped");

        [HarmonyPostfix]
        [HarmonyPatch("OnChairOccupied")]
        static void OnChairOccupied(ChairBase chair)
        {
            if (chair.photonView.Owner != PhotonNetwork.LocalPlayer || chair is TurretChair || chair is not TakeoverChair ||
                chair == ClientGame.Current.PlayerShip.gameObject.GetComponentInChildren<Helm>().Chair) return;

            playerChair = chair;
            ControllingHelmPatch.controllingHelm.AbilityActivator = ClientGame.Current.PlayerShip.gameObject.GetComponentInChildren<Helm>();
            isHelm = false;
            AbilityStartedMethod.Invoke(ControllingHelmPatch.controllingHelm, null);
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnChairFreed")]
        static void OnChairFreed(ChairBase chair)
        {
            if (chair != playerChair) return;

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
