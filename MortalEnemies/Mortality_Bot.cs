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

		private float storedConstantGravity = 2f, storedGravity = 80f; // these values are only assigned to make the compiler happy and to cover edge cases, should be copied upon death

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

			if (botRef is null)
			{
				playerRef = null;
				return; // Sanity check
			}

			playerRef = gameObject.GetComponentInChildren<Player>();

			componentsToDeactivate.Add(botRef);
			foreach (MonoBehaviour tempMono in gameObject.GetComponentsInChildren<MonoBehaviour>())
			{
				if (tempMono.GetType().ToString().Contains("Bot_") || tempMono.GetType().ToString().Contains("Attack")) componentsToDeactivate.Add(tempMono);
			}

			// Logging
			foreach (MonoBehaviour tempMono in componentsToDeactivate)
			{
				string tempType = (tempMono is null || tempMono.GetType() is null || tempMono.GetType().ToString() is null) ? "null" : tempMono.GetType().ToString();
			}
		}

		public override float Health
		{
			get { return health; }
			internal set
			{
				if (health > 0f && value <= 0f) KillEffect();
				health = Mathf.Min(value, MaxHealth);
			}
		}

		public override float MaxHealth
		{
			get { return maxHealth; }
			internal set { maxHealth = value; }
		}

		protected override void KillEffect()
		{
			health = 0f;

			// Sanity check - base references
			if (botRef is null || componentsToDeactivate is null) return;
			BotHandler.instance.bots.Remove(botRef);

			// Cancel out any inputs that the AI last prescribed
			botRef.DoNothing();

			// Sanity check - reference to a Player
			if (playerRef is not null)
			{
				playerRef.data.dead = true; // Required for ragdolling and tracking time since death

				// Sanity check - reference to a Ragdoll
				if (playerRef.refs.ragdoll is not null)
				{
					//playerRagdoll.ToggleSimplifiedRagdoll(true); // causes gravity bugs
				}

				// Sanity check - reference to a Controller
				if (playerRef.refs.controller is not null)
				{
					storedConstantGravity = playerRef.refs.controller.constantGravity;
					storedGravity = playerRef.refs.controller.gravity;

					playerRef.refs.controller.constantGravity = 4f;
					playerRef.refs.controller.gravity = 80f;
				}
			}

			// Deactivate Components
			foreach (MonoBehaviour tempComponent in componentsToDeactivate)
			{
				if (tempComponent is not null) tempComponent.enabled = false;
			}
		}

		protected override void ReviveEffect()
		{
			// Sanity check - base references
			if (botRef == null || componentsToDeactivate == null) return;
			BotHandler.instance.bots.Add(botRef);

			// Activate Components
			foreach (MonoBehaviour tempComponent in componentsToDeactivate)
			{
				if (tempComponent is not null) tempComponent.enabled = true;
			}

			// Sanity check - reference to a Player
			if (playerRef is not null)
			{
				playerRef.data.dead = false;

				// Sanity check - reference to a Ragdoll
				if (playerRef.refs.ragdoll is not null)
				{
					//playerRagdoll.ToggleSimplifiedRagdoll(false); // causes gravity bugs
				}

				// Sanity check - reference to a Controller
				if (playerRef.refs.controller is not null)
				{
					playerRef.refs.controller.constantGravity = storedConstantGravity;
					playerRef.refs.controller.gravity = storedGravity;
				}
			}
		}
	}
}