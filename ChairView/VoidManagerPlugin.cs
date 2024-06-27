﻿using VoidManager.MPModChecks;

namespace ChairView
{
    public class VoidManagerPlugin : VoidManager.VoidPlugin
    {
        static VoidManagerPlugin()
        {
            VoidManager.Events.Instance.JoinedRoom += OnChairSeatedChangedPatch.Reset;
            VoidManager.Events.Instance.LeftRoom += OnChairSeatedChangedPatch.Reset;
        }

        public override MultiplayerType MPType => MultiplayerType.Client;

        public override string Author => "18107";

        public override string Description => "Pressing switch view (TAB) while in a chair shows the ship exterior view";
    }
}
