using MyceliumNetworking;
using System;
using MonoMod.RuntimeDetour.HookGen;
using HarmonyLib;
using CessilCellsCeaChells.CeaChore;

//[assembly: RequiresMethod(typeof(Bot), "Awake", typeof(void))]

namespace MortalEnemies.Patches
{
	public class ClassPatches
	{
		internal static void Init()
		{
			// Hook to Bot.Awake() created by CCCC
			HookEndpointManager.Add(AccessTools.Method(typeof(Bot), "Start"), HookBotAwake);
			HookEndpointManager.Add(AccessTools.Method(typeof(Player), "Awake"), HookPlayerAwake);

			// Register mod to Mycelium


			// Subscribe to Mycelium events
			MyceliumNetwork.LobbyEntered += Mortality.UpdateNetworkState;
			MyceliumNetwork.LobbyLeft += Mortality.UpdateNetworkState;
		}

		public static void HookBotAwake(Action<Bot> orig, Bot self)
		{
			if (self.GetType() != typeof(Bot)) return; // safety

			if (self.hideFlags == UnityEngine.HideFlags.HideAndDontSave || !self.isActiveAndEnabled)
			{
				MortalEnemies.Logger.LogDebug("Not Adding mortality because object is not active");
				return;
			}

			MortalEnemies.Logger.LogDebug(" ");
			MortalEnemies.Logger.LogDebug("Adding Mortality to " + self.gameObject.name);

			// Run original Awake() function
			orig(self);

			// Create a mortality component and attach to same GameObject as Bot
			if (self.gameObject.GetComponent<Mortality>() != null)
			{
				MortalEnemies.Logger.LogDebug("Tried adding additional Mortality component to same object");
				return;
			}

			Mortality_Bot newMortality = self.gameObject.AddComponent<Mortality_Bot>();
			newMortality?.SetBot(self);
		}

		public static void HookPlayerAwake(Action<Player> orig, Player self)
		{
			// Run original Awake() function
			orig(self);

			// Sanity check
			if (self.ai) return;

			MortalEnemies.Logger.LogDebug("Adding Mortality to " + self.gameObject.name);
			// Create a mortality component and attach to same GameObject as Player
			Mortality_Player newMortality = self.gameObject.AddComponent<Mortality_Player>();
			if (newMortality is null) MortalEnemies.Logger.LogDebug("Failed to add Mortality component!");
			newMortality?.SetPlayer(self);
		}
	}
}
