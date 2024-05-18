using System.Collections.Generic;
using UnityEngine;

namespace MortalEnemies
{
	public class Mortality_Bot : Mortality
	{
		private float health, maxHealth = 100f;
		private Bot? botRef;
		private Player? playerRef;
		private HashSet<MonoBehaviour>? componentsToDeactivate;

		protected override void Awake()
		{
			base.Awake();
			health = maxHealth;
			MortalSingleton.Instance.BotMortalities.Add(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			MortalSingleton.Instance.BotMortalities.Remove(this);
		}

		internal void SetBot(Bot newBotRef)
		{
			botRef = newBotRef;
			componentsToDeactivate = new(); // Clear hashset

			if (botRef == null)
			{
				playerRef = null;
				return; // Sanity check
			}

			playerRef = GetComponentInChildren<Player>();
			if (playerRef = null) MortalEnemies.Logger.LogDebug("Ragdolling not supported");

			componentsToDeactivate.Add(botRef);
			foreach (MonoBehaviour tempMono in botRef.GetComponentsInChildren<MonoBehaviour>())
			{
				if (tempMono.GetType().ToString().Contains("Bot_") && !tempMono.GetType().ToString().Contains("Ragdoll")) componentsToDeactivate.Add(tempMono);
			}
			componentsToDeactivate.Add(botRef.transform.parent.gameObject.GetComponentInChildren<PlayerRagdoll>());

			// Logging
			MortalEnemies.Logger.LogDebug("List of Monobehaviours to deactivate:");
			foreach (MonoBehaviour tempMono in componentsToDeactivate)
			{
				string tempType = (tempMono is null || tempMono.GetType() is null || tempMono.GetType().ToString() is null) ? "null" : tempMono.GetType().ToString();
				MortalEnemies.Logger.LogDebug($"  └{containerObjectName} -> {tempType}");
			}

			// Debug HUD
			//MortalityHUDSource newSource = newBotRef.gameObject.AddComponent<MortalityHUDSource>();
			//newSource.SetMortality(this);
		}

		public override float Health
		{
			get { return health; }
			internal set
			{
				if (IsAlive && value <= 0f) KillEffect();
				health = Mathf.Max(value, 0f);
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

			BotHandler.instance.bots.Remove(botRef);

			foreach (MonoBehaviour tempComponent in componentsToDeactivate)
			{
				MortalEnemies.Logger.LogDebug($"  └{containerObjectName} -> {tempComponent.GetType()} disabled");
				tempComponent.enabled = false;
			}
			botRef.DoNothing();

			if (playerRef is not null)
			{
				PlayerRagdoll playerRagdoll = playerRef.refs.ragdoll;
				if (playerRagdoll is null) MortalEnemies.Logger.LogDebug("Could not find ragdoll component!");
				else
				{
					playerRagdoll.ToggleSimplifiedRagdoll(true);
					playerRagdoll.ToggleGravity(true);
				}
			}
			// TODO trigger ragdoll
		}

		protected override void ReviveEffect()
		{
			if (botRef == null || componentsToDeactivate == null) return;

			BotHandler.instance.bots.Add(botRef);

			if (playerRef is not null)
			{
				PlayerRagdoll playerRag = playerRef.GetComponent<PlayerRagdoll>();
				if (playerRag is null) MortalEnemies.Logger.LogDebug("Could not find ragdoll component!");
				else
				{
					playerRag.ToggleSimplifiedRagdoll(false);
					playerRag.ToggleGravity(false);
				}
			}

			foreach (MonoBehaviour tempComponent in componentsToDeactivate)
			{
				MortalEnemies.Logger.LogDebug($"  └{containerObjectName} -> {tempComponent.GetType()} enabled");
				tempComponent.enabled = true;
			}
			// TODO trigger unragdoll
		}
	}
}