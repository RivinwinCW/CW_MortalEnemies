using BepInEx;
using BepInEx.Logging;
using MonoMod.RuntimeDetour.HookGen;
using MortalEnemies.Patches;
using System.Reflection;

namespace MortalEnemies
{
	[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
	public class MortalEnemies : BaseUnityPlugin
	{
		public static MortalEnemies Instance { get; private set; } = null!;
		internal new static ManualLogSource Logger { get; private set; } = null!;

		private void Awake()
		{
			Instance = this;
			Logger = base.Logger;

			Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} installed, hooking...");
			
			HookAll();
		}

		internal static void HookAll()
		{
			ClassPatches.Init();

			Logger.LogDebug("Finished hooking");
		}

		internal static void UnhookAll()
		{
			HookEndpointManager.RemoveAllOwnedBy(Assembly.GetExecutingAssembly());

			Logger.LogDebug("Finished unhooking");
		}
	}
}
