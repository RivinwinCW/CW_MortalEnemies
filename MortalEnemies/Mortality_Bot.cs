using MyceliumNetworking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MortalEnemies
{
	public class Mortality_Bot : Mortality
	{
		private float health, maxHealth = 100f;
		private Bot? botRef;
		private HashSet<MonoBehaviour>? componentsToDeactivate;

		protected override void Awake()
		{
			base.Awake();
			health = maxHealth;
			MortalSingleton.Instance.BotMortalities.Add(this);
		}

		internal void SetBot(Bot newBotRef)
		{
			botRef = newBotRef;
			componentsToDeactivate = new(); // Clear hashset

			if (botRef == null) return; // Sanity check

			componentsToDeactivate.Add(botRef);
			foreach (MonoBehaviour tempMono in botRef.GetComponents<MonoBehaviour>())
			{
				if (tempMono.GetType().ToString().Contains("Bot_") && !tempMono.GetType().ToString().Contains("Ragdoll")) componentsToDeactivate.Add(tempMono);
			}

			// Logging
			MortalEnemies.Logger.LogDebug("List of Monobehaviours to deactivate:");
			foreach (MonoBehaviour tempMono in componentsToDeactivate) MortalEnemies.Logger.LogDebug($"  └{tempMono.transform.parent.gameObject.name} -> {tempMono.GetType().ToString()}");

			// Debug HUD
			MortalityHUDSource newSource = newBotRef.gameObject.AddComponent<MortalityHUDSource>();
			newSource.SetMortality(this);
		}

		public override float Health
		{
			get { return health; }
			internal set
			{
				health = value;
				if (health <= 0f) KillEffect();
				health = 0f;
			}
		}

		public override float MaxHealth
		{
			get { return maxHealth; }
			internal set { maxHealth = value; }
		}

		protected override void KillEffect()
		{
			if (botRef == null || componentsToDeactivate == null) return;

			foreach (MonoBehaviour tempComponent in componentsToDeactivate)
			{
				MortalEnemies.Logger.LogDebug($"  └{tempComponent.gameObject.name} -> {tempComponent.GetType().ToString()} disabled");
				tempComponent.enabled = false;
			}
			botRef.DoNothing();
			// TODO trigger ragdoll
		}

		protected override void ReviveEffect()
		{
			if (botRef == null || componentsToDeactivate == null) return;

			foreach (MonoBehaviour tempComponent in componentsToDeactivate)
			{
				MortalEnemies.Logger.LogDebug($"  └{tempComponent.gameObject.name} -> {tempComponent.GetType().ToString()} enabled");
				tempComponent.enabled = true;
			}
			// TODO trigger unragdoll
		}
	}
}