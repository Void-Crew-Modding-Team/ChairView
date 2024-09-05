using VoidManager.MPModChecks;

namespace ChairView
{
    public class VoidManagerPlugin : VoidManager.VoidPlugin
    {
        static VoidManagerPlugin()
        {
            VoidManager.Events.Instance.JoinedRoom += ChairBasePatch.Reset;
            VoidManager.Events.Instance.LeftRoom += ChairBasePatch.Reset;
        }

        public override MultiplayerType MPType => MultiplayerType.Client;

        public override string Author => MyPluginInfo.PLUGIN_AUTHORS;

        public override string Description => MyPluginInfo.PLUGIN_DESCRIPTION;

        public override string ThunderstoreID => MyPluginInfo.PLUGIN_THUNDERSTORE_ID;
    }
}
