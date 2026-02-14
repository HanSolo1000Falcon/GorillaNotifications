using BepInEx;
using GorillaNotifications.Core;

namespace GorillaNotifications;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
internal class Plugin : BaseUnityPlugin
{
    private void Start() => GorillaTagger.OnPlayerSpawned(() => gameObject.AddComponent<NotificationController>());
}