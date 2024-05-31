using Mono.Cecil;
using MyceliumNetworking;
using Photon.Pun;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace MortalEnemies
{
	public abstract class Mortality : MonoBehaviour
	{
		// LIST OF ABSTRACTS
		/* Health
		 * MaxHEalth
		 * KillEffect
		 * ReviveEffect
		 */

		// LIST OF VIRTUALS
		/* DamageEffect
		 */

		// CONSTANTS
		public static readonly uint modId = (uint)MyPluginInfo.PLUGIN_GUID.GetHashCode(); // was suggested going to unity.hash128

		// Networking Variables
		public int viewIDClone;
		private bool isAutonomousProxy;
		private MortalSingleton mortalSingleton = MortalSingleton.Instance; // We mostly cache this because its used in FixedUpdate();

		// DoT Variables
		private enum TimedEffectState
		{
			None,
			Damaging,
			Healing
		}
		private TimedEffectState uiEffectState;
		private List<DoTSource> dotSources = new();
		private static ushort nextDoTSourceID;
		internal float damagePerTick;
		private bool damagePerTickDirty;

		// Health Variables
		public bool IsAlive => Health > 0f;
		public abstract float MaxHealth { get; internal set; }
		public abstract float Health { get; internal set; }

		// Events
		protected virtual void Awake()
		{
			// Add this mortality to the list stored in the singleton
			mortalSingleton.Mortalities.Add(this);

			// Register with Mycelium
			viewIDClone = GetComponent<PhotonView>().ViewID;
			MyceliumNetwork.RegisterNetworkObject(this, modId, viewIDClone);
		}

		private void FixedUpdate() // By using FixedUpdate() as the basis for the DoT system it should be compatible with slow-motion/bullet-time mods
		{
			// Handle damage over time
			if (dotSources.Count != 0)
			{
				// Clear old dots and recalculate damage per tick if necessary
				while (dotSources.Count > 0 && dotSources[0].expireTick < mortalSingleton.CurrentTick) // Assumes that dotSources is always sorted soonest to latest by expireTick
				{
					dotSources.RemoveAt(0);
					damagePerTickDirty = true;
				}
				if (damagePerTickDirty) CalcDamagePerTick();

				// Trigger effects
				if (uiEffectState == TimedEffectState.Damaging) DoTEffect();
				else if (uiEffectState == TimedEffectState.Healing && Health < MaxHealth) HoTEffect();

				// Apply damage
				Health -= damagePerTick;
			}
		}

		protected virtual void OnDestroy()
		{
			MyceliumNetwork.DeregisterNetworkObject(this, modId, viewIDClone);
			mortalSingleton.Mortalities.Remove(this);
		}

		

		// PUBLIC METHODS TO CALL FROM OTHER MODS

		// Requests that UI, Material, and Audio cues be played
		public virtual void DamageEffect()
		{

		}

		public virtual void DoTEffect()
		{

		}

		public virtual void HealEffect()
		{

		}

		public virtual void HoTEffect()
		{

		}

		protected abstract void KillEffect();
		protected abstract void ReviveEffect();

		// Applys Damage or Healing on the Host and propigates to Clients, triggers effects on both
		public void Damage(float inDamage)
		{
			if (inDamage <= 0f) return;

			if (isAutonomousProxy) RPCA_Damage(inDamage); // Run locally if autonomous
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_Damage), ReliableType.Reliable, viewIDClone, inDamage); // Run on all machines if Authorized
		}

		public ushort DamageOverTime(float inDamagePerSecond, float inSeconds) // returns the ID of the source created
		{
			if (!isAutonomousProxy && !MyceliumNetwork.IsHost) return 0; // Sanity check - returns 0 as error code

			ushort newID = GetUnigueDoTSourceID();
			uint newTicks = (ushort)Mathf.RoundToInt(inSeconds / Time.fixedDeltaTime);
			float tempDpT = inDamagePerSecond * Time.fixedDeltaTime;

			if (isAutonomousProxy) RPCA_AddDoTSource(tempDpT, newTicks, newID);
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_AddDoTSource), ReliableType.Reliable, viewIDClone, tempDpT, newTicks, newID);
			return newID;
		}

		public void Heal(float inHealth)
		{
			if (inHealth <= 0f) return; // Sanity check

			if (isAutonomousProxy) RPCA_Heal(inHealth);
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_Heal), ReliableType.Reliable, viewIDClone, inHealth);
		}

		public ushort HealOverTime(float inHealthPerSecond, float inSeconds)
		{
			return DamageOverTime(-inHealthPerSecond, inSeconds);
		}

		public void Revive(float newHealth = 100f)
		{
			if (newHealth <= 0f) return; // Sanity check
			if (newHealth > MaxHealth) newHealth = MaxHealth;

			if (isAutonomousProxy) RPCA_Revive(newHealth);
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_Revive), ReliableType.Reliable, viewIDClone, newHealth);
		}

		private static ushort GetUnigueDoTSourceID()
		{
			if (!MyceliumNetwork.IsHost) return 0; // proxy dot sources will have an id of 0, TODO: need to implement a system to remove them using client level IDs
			return ++nextDoTSourceID; // because we preincrement it should never be 0 for networked sources
		}

		// Can be referenced from other mods so that a DoTSource can be removed as well as created using a reference
		public class DoTSource
		{
			internal float damagePerTick;
			internal uint expireTick, ticksRemaining; // ticksRemaining/expireTime are not always synced and safe!
			internal ushort ID;
			internal DoTSource(float newDpT, uint newTicks, ushort newID)
			{
				damagePerTick = newDpT;
				ticksRemaining = newTicks;
				expireTick = MortalSingleton.Instance.CurrentTick + newTicks;
				ID = newID;
			}

			internal bool paused;
			internal bool Paused
			{
				get { return paused; }
				set
				{
					if (paused == value) return; // only do following logic if value is actually changing
					if (value) ticksRemaining = expireTick - MortalSingleton.Instance.CurrentTick; // I am sure i have a -1 falicy error in here somewhere, testing needed
					else expireTick = MortalSingleton.Instance.CurrentTick + ticksRemaining;
					paused = value;
				}
			}
		}

		public void RemoveDot(ushort IDtoRemove)
		{
			if (isAutonomousProxy) RPCA_RemoveDoTSource(IDtoRemove);
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_RemoveDoTSource), ReliableType.Reliable, viewIDClone, IDtoRemove);
		}
		public void PauseDoT(ushort IDtoPause)
		{
			if (isAutonomousProxy) RPCA_PauseDoTSource(IDtoPause);
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_PauseDoTSource), ReliableType.Reliable, viewIDClone, IDtoPause);
		}
		public void ResumeDoT(ushort IDtoResume)
		{
			if (isAutonomousProxy) RPCA_ResumeDoTSource(IDtoResume);
			else if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RPCA_ResumeDoTSource), ReliableType.Reliable, viewIDClone, IDtoResume);
		}
		protected virtual void CalcDamagePerTick()
		{
			// Reset variable, then sum up
			damagePerTick = 0f;
			foreach (DoTSource tempDoT in dotSources) if (!tempDoT.paused) damagePerTick += tempDoT.damagePerTick;
			damagePerTickDirty = false;

			// Update whether damage/heal over time effects are valid
			if (Mathf.Abs(damagePerTick) > 0.015) uiEffectState = (damagePerTick > 0 ? TimedEffectState.Damaging : TimedEffectState.Healing);
			else uiEffectState = TimedEffectState.None;
		}

		// RPCs
		[CustomRPC]
		internal void RPCA_Damage(float sentDamage)
		{
			DamageEffect(); // Trigger ui, particle, materials effects etc
			Health -= sentDamage;
		}

		[CustomRPC]
		internal void RPCA_Heal(float sentHealth)
		{
			HealEffect();
			Health += sentHealth;
		}

		[CustomRPC]
		internal void RPCA_Revive(float sentHealth)
		{
			Health = sentHealth;
			ReviveEffect();
		}

		[CustomRPC]
		internal void RPCA_AddDoTSource(float sentDpT, uint sentTicks, ushort sentID)
		{
			DoTSource newSource = new DoTSource(sentDpT, sentTicks, sentID);
			// get dots from the end of the list and check if they expire before our new dot, if they do, insert after it
			for (int i = dotSources.Count - 1; i >= 0; i--)
			{
				if (dotSources[i].expireTick <= newSource.expireTick)
				{
					// insert the new dot in the middle/end
					dotSources.Insert(i + 1, newSource);
					damagePerTickDirty = true;
					return;
				}
			}
			// if none are found to expire sooner, insert at the beginning
			dotSources.Insert(0, newSource);
			damagePerTickDirty = true;
		}

		[CustomRPC]
		internal void RPCA_RemoveDoTSource(ushort toRemove)
		{
			foreach (DoTSource tempSource in dotSources)
			{
				if (tempSource.ID == toRemove)
				{
					dotSources.Remove(tempSource);
					damagePerTickDirty = true;
					break;
				}
			}
		}

		[CustomRPC]
		internal void RPCA_PauseDoTSource(ushort toPause)
		{
			foreach (DoTSource tempSource in dotSources)
			{
				if (tempSource.ID == toPause)
				{
					tempSource.Paused = true;
					damagePerTickDirty = true;
					break;
				}
			}
		}

		[CustomRPC]
		internal void RPCA_ResumeDoTSource(ushort toResume)
		{
			foreach (DoTSource tempSource in dotSources)
			{
				if (tempSource.ID == toResume)
				{
					tempSource.Paused = false;
					damagePerTickDirty = true;
					break;
				}
			}
		}
	}
}
