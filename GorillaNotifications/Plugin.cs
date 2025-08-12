using BepInEx;
using GorillaNotifications.Notifications;

namespace GorillaNotifications
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private void Start() => GorillaTagger.OnPlayerSpawned(OnGameInitialized);

        private void OnGameInitialized()
        {
            gameObject.AddComponent<OnScreenNotifications>();
        }
    }
}