using UnityEngine;

namespace MortalEnemies
{
	internal class Mortality_Player : Mortality
	{
		protected float nextDoTEffects, nextDamageEffects, nextHealEffects;
		

		internal bool attachedToLocalPlayer;
		internal Player? playerRef;
		internal Player.PlayerData? playerData;

		//DoT
		protected float dotEffectInterval = .75f, damageEffectInterval = .09f, healEffectInterval = .09f;

		internal void SetPlayer(Player newPlayerRef)
		{
			if (newPlayerRef == playerRef || newPlayerRef == null) return;

			attachedToLocalPlayer = (newPlayerRef == Player.localPlayer);

			playerRef = newPlayerRef;
			playerData = playerRef.data;
		}

		public override float Health
		{
			get { return playerData == null ? 100f : playerData.health; }
			internal set
			{
				if (playerData is null) return;
				playerData.health = Mathf.Min(value, MaxHealth);
			}
		}

		public override float MaxHealth
		{
			get { return 100f; }
			internal set { }
		}

		// PUBLIC METHODS
		public override void DamageEffect()
		{
			if (Time.time > nextDamageEffects)
			{
				UI_Feedback.instance.TakeDamage();
				nextDamageEffects = Time.time + damageEffectInterval;
			}
		}

		public override void DoTEffect()
		{
			if (Time.time > nextDoTEffects)
			{
				UI_Feedback.instance.TakeDamage();
				nextDoTEffects = Time.time + dotEffectInterval;
			}
		}

		public override void HealEffect()
		{
			if (Time.time > nextHealEffects)
			{
				UI_Feedback.instance.HealFeedback();
				nextHealEffects = Time.time + healEffectInterval;
			}
		}

		public override void HoTEffect()
		{
			if (Time.time > nextDoTEffects)
			{
				UI_Feedback.instance.HealFeedback();
				nextDoTEffects = Time.time + dotEffectInterval;
			}
		}

		protected override void KillEffect()
		{
			playerData.player.Die();
		}

		protected override void ReviveEffect()
		{
			if (!playerData.dead) return;

			playerData.dead = false;

			if (attachedToLocalPlayer)
			{
				Player.justDied = false;
				NetworkVoiceHandler.TalkToAlive();
				UI_Feedback.instance.Revive();
			}
			if (!PlayerHandler.instance.playersAlive.Contains(playerData.player))
			{
				PlayerHandler.instance.playersAlive.Add(playerData.player);
			}
		}
	}
}
