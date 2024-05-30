using MyceliumNetworking;
using System;
using MonoMod.RuntimeDetour.HookGen;
using HarmonyLib;
using CessilCellsCeaChells.CeaChore;
using UnityEngine;

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

			// Subscribe to Mycelium events
			MyceliumNetwork.LobbyEntered += Mortality.UpdateNetworkState;
			MyceliumNetwork.LobbyLeft += Mortality.UpdateNetworkState;
		}

		public static void HookBotAwake(Action<Bot> orig, Bot self)
		{
			// Determine new parent GameObject
			GameObject newParent = self.gameObject;
			while (!newParent.name.Contains("(Clone)") && newParent.transform.parent is not null)
			{
				newParent = newParent.transform.parent.gameObject; // move upwards through the tree starting at the current gameobject till at root or the name contains (Clone)
			}

			// Run original Awake() function
			orig(self);

			// Create a mortality component and attach to same GameObject as Bot
			if (newParent.GetComponent<Mortality>() is not null) return;

			Mortality_Bot newMortality = newParent.AddComponent<Mortality_Bot>();
			newMortality?.SetBot(self);
		}

		public static void HookPlayerAwake(Action<Player> orig, Player self)
		{
			// Run original Awake() function
			orig(self);

			// Sanity check
			if (self.ai) return;

			// Create a mortality component and attach to same GameObject as Player
			Mortality_Player newMortality = self.gameObject.AddComponent<Mortality_Player>();
			newMortality?.SetPlayer(self);
		}
	}
}
