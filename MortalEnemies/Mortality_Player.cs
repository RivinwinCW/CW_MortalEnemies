using UnityEngine;

namespace MortalEnemies
{
	internal class Mortality_Player : Mortality
	{
		protected float nextDoTEffects, nextDamageEffects, nextHoTEffects, nextHealEffects;
		protected float DoTEffectInterval = 1f, DamageEffectInterval = .2f, HoTEffectInterval = 1f, HealEffectInterval = .2f;

		internal bool attachedToLocalPlayer;
		internal Player? playerRef;
		internal Player.PlayerData? playerData;

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
				playerData.health = value;
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
				nextDamageEffects = Time.time + DamageEffectInterval;
			}
		}

		public override void DoTEffect()
		{
			if (Time.time > nextDoTEffects)
			{
				UI_Feedback.instance.TakeDamage();
				nextDoTEffects = Time.time + DoTEffectInterval;
			}
		}

		public override void HealEffect()
		{
			if (Time.time > nextHealEffects)
			{
				UI_Feedback.instance.HealFeedback();
				nextHealEffects = Time.time + HealEffectInterval;
			}
		}

		public override void HoTEffect()
		{
			if (Time.time > nextDoTEffects)
			{
				UI_Feedback.instance.HealFeedback();
				nextHoTEffects = Time.time + HoTEffectInterval;
			}
		}

		protected override void KillEffect()
		{
			playerData.player.Die();
		}

		protected override void ReviveEffect()
		{
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
