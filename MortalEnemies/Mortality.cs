using MyceliumNetworking;
using Photon.Pun;
using System.Collections.Generic;
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

		// CONSTANTS
		public static readonly uint modId = (uint)MyPluginInfo.PLUGIN_GUID.GetHashCode(); // was suggested going to unity.hash128

		// DoT Variables
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
			// Create Singleton if it doesnt exist
			MortalSingleton tempSingletonRef = MortalSingleton.Instance;

			// Set up networking variables
			viewIDClone = GetComponent<PhotonView>().ViewID;

			MortalEnemies.Logger.LogDebug("Network info: modID: " + modId + ", viewIDClone: " + viewIDClone);

			// Add this mortality to the list stored in the singleton
			MortalSingleton.Instance.Mortalities.Add(this);
			//MortalSingleton.Instance.LogNumMortalities();

			MyceliumNetwork.RegisterNetworkObject(this, modId, viewIDClone);
		}

		private void FixedUpdate()
		{
			// Handle damage over time
			if (dotSources.Count != 0)
			{
				// Clear old dots and recalculate damage per tick if necessary
				while (dotSources.Count > 0 && dotSources[0].expireTime < Time.time) // Assumes that dotSources is always sorted soonest to latest by expireTime
				{
					dotSources.RemoveAt(0);
					damagePerTickDirty = true;
				}
				if (damagePerTickDirty) CalcDamagePerTick();

				// Trigger effects
				if (damagePerTick < 0f) HoTEffect();
				else DoTEffect();

				// Apply damage
				Health -= damagePerTick;
			}
		}

		private void OnDestroy()
		{
			MyceliumNetwork.DeregisterNetworkObject(this, modId, viewIDClone);
			MortalSingleton.Instance.Mortalities.Remove(this);
		}

		// NETWORKING
		public static bool Authorized => _authority > NetworkState.Client;

		public int viewIDClone; // photon view id for mycelium object masking
		private enum NetworkState
		{
			Offline,
			Client,
			Autonamous,
			Server
		}

		private static NetworkState _authority = NetworkState.Offline;
		internal static void UpdateNetworkState()
		{
			if (MyceliumNetwork.IsHost)	_authority = NetworkState.Server;
			else if (MyceliumNetwork.InLobby) _authority = NetworkState.Client;
			else _authority = NetworkState.Offline;
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
			if (inDamage <= 0f || !Authorized)
			{
				MortalEnemies.Logger.LogDebug($"Not authorized to deal damage to to {this.gameObject.name}");
				return; // Sanity check
			}
			DamageEffect(); // Trigger ui, particle, materials effects etc

			Health -= inDamage;

			if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(Damage_RPC), ReliableType.Reliable, viewIDClone, Health);
		}

		public DoTSource DamageOverTime(float inDamagePerSecond, float inSeconds)
		{
			if (!Authorized || inDamagePerSecond * inSeconds <= 0f) return null; // Sanity check
			DoTEffect(); // Trigger ui, particle, materials effects etc
			ushort newID = GetUnigueDoTSourceID();

			// In testing this assignment was necessary, else Mycelium throws a casting exception, would like to remove
			float tempDPS = inDamagePerSecond;
			float tempSeconds = inSeconds;

			DoTSource resultSource = AddDoT(tempDPS, tempSeconds, newID);

			if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(DamageOverTime_RPC), ReliableType.Reliable, viewIDClone, tempDPS, tempSeconds, newID);
			return resultSource;
		}

		public void Heal(float inHealth = 0f)
		{
			if (!Authorized || inHealth <= 0f) return; // Sanity check
			HealEffect();
			Health += inHealth;

			if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(Heal_RPC), ReliableType.Reliable, viewIDClone, Health);
		}

		public DoTSource HealOverTime(float inHealthPerSecond, float inSeconds)
		{
			if (!Authorized || inHealthPerSecond * inSeconds <= 0f) return null; // Sanity check
			HoTEffect();
			ushort newID = GetUnigueDoTSourceID();

			// In testing this assignment was necessary, else Mycelium throws a casting exception, would like to remove
			float tempDPS = -inHealthPerSecond;
			float tempSeconds = inSeconds;

			DoTSource resultSource = AddDoT(tempDPS, tempSeconds, newID);
			
			if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(DamageOverTime_RPC), ReliableType.Reliable, viewIDClone, tempDPS, tempSeconds, newID);
			return resultSource;
		}

		public void Revive(float newHealth = 100f)
		{
			if (!Authorized) return;
			
			ReviveEffect();

			newHealth = Mathf.Clamp(newHealth, 0.01f, MaxHealth);
			Health = newHealth;

			if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(Revive_RPC), ReliableType.Reliable, viewIDClone, Health);
		}

		private ushort GetUnigueDoTSourceID()
		{
			if (!MyceliumNetwork.IsHost) return 0;
			return ++nextDoTSourceID;
		}

		// Can be referenced from other mods so that a DoTSource can be removed as well as created using a reference
		public class DoTSource
		{
			internal float damagePerTick, expireTime;
			internal ushort ID;
			internal DoTSource(float newDamagePerSecond, float newDuration, ushort newID)
			{
				damagePerTick = newDamagePerSecond * Time.fixedDeltaTime;
				expireTime = Time.time + newDuration;
				ID = newID;
			}
		}

		private DoTSource AddDoT(float dps, float duration, ushort newID)
		{
			DoTSource newDoT = new DoTSource(dps, duration, newID);
			AddDoT(newDoT);
			return newDoT;
		}

		private void AddDoT(DoTSource newSource)
		{
			// get dots from the end of the list and check if they expire before our new dot
			for (int i = dotSources.Count - 1; i >= 0; i--)
			{
				if (dotSources[i].expireTime < newSource.expireTime)
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

		public void RemoveDot(DoTSource toRemove)
		{
			if (!Authorized) return;
			dotSources.Remove(toRemove);

			MortalEnemies.Logger.LogDebug("viewIDClone: " + viewIDClone);

			if (MyceliumNetwork.IsHost) MyceliumNetwork.RPCMasked(modId, nameof(RemoveDoTSource_RPC), ReliableType.Reliable, viewIDClone, toRemove.ID);
		}

		private void CalcDamagePerTick()
		{
			damagePerTick = 0f;
			foreach (DoTSource tempDoT in dotSources) damagePerTick += tempDoT.damagePerTick;
			damagePerTickDirty = false;
		}

		// RPCs - called by server on clients
		[CustomRPC]
		private void Damage_RPC(float serverHealth)
		{
			DamageEffect();
			Health = serverHealth;
		}

		[CustomRPC]
		private void DamageOverTime_RPC(float damagePerSecond, float duration, ushort ID)
		{
			AddDoT(damagePerSecond, duration, ID);
		}

		[CustomRPC]
		private void Heal_RPC(float serverHealth)
		{
			Heal();
			Health = serverHealth;
		}

		[CustomRPC]
		private void Revive_RPC(float serverHealth)
		{
			Revive();
			Health = serverHealth;
		}

		[CustomRPC]
		private void RemoveDoTSource_RPC(ushort toRemove)
		{
			foreach (DoTSource tempSource in dotSources)
			{
				if (tempSource.ID == toRemove)
				{
					dotSources.Remove(tempSource);
					break;
				}
			}
		}
	}
}
