using System.Collections.Generic;
using UnityEngine;

namespace MortalEnemies
{
	public class MortalSingleton : MonoBehaviour
	{
		private static KeyCode[] debugKeys =
		{
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.Alpha0
		};

		private static MortalSingleton? _instance;
		public static MortalSingleton Instance
		{
			get
			{
				if (_instance == null) CreateMortalitySingleton();
				return _instance;
			}
		}

		private HashSet<Mortality> _mortalities = new();
		private HashSet<Mortality> _botMortalities = new();
		public HashSet<Mortality> Mortalities
		{
			get { return _mortalities; }
			private set { _mortalities = value; }
		}
		public HashSet<Mortality> BotMortalities
		{
			get { return _botMortalities; }
			private set { _botMortalities = value; }
		}

		public static void CreateMortalitySingleton()
		{
			_instance = GameObject.FindFirstObjectByType<MortalSingleton>();
			if (_instance != null) return;

			GameObject tempObject = new GameObject("MortalSingleton", typeof(MortalSingleton));
			DontDestroyOnLoad(tempObject);
		}

		public void LogNumMortalities()
		{
			if (_mortalities.Count == 0)
			{
				MortalEnemies.Logger.LogDebug("No Mortality components currently online");
			} else
			{
				MortalEnemies.Logger.LogDebug("Mortality Components: " + _mortalities.Count);
				foreach (Mortality tempMortality in _mortalities) MortalEnemies.Logger.LogDebug("  └" + tempMortality.gameObject.name);
			}
		}

		void Awake()
		{
			// Set _instance following singleton pattern
			if (_instance == null) _instance = this;
			else if (_instance != this) // Error state - singleton already exists
			{
				MortalEnemies.Logger.LogDebug("MortalSingleton already exists, destroying duplicate");
				Destroy(this);
				return;
			}

			foreach (Mortality tempMort in FindObjectsOfType<Mortality>()) _mortalities.Add(tempMort);
			MortalEnemies.Logger.LogDebug("MortalSingleton created");
			//LogNumMortalities();
		}

		void Update()
		{
			// Debug keys only work when holding CTRL
			//if (!(GlobalInputHandler.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))) return;

			// Debug key: Test Player Damage
			if (GlobalInputHandler.GetKeyUp(debugKeys[3]))
			{
				MortalEnemies.Logger.LogDebug("Test - Player Damage");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.Damage(25f);
			}

			// Debug key: Test Player Damage Over Time
			if (GlobalInputHandler.GetKeyUp(debugKeys[4]))
			{
				MortalEnemies.Logger.LogDebug("Test - Player Damage Over Time");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.DamageOverTime(5f, 5f);
			}

			// Debug key: Test Player Heal
			if (GlobalInputHandler.GetKeyUp(debugKeys[5]))
			{
				MortalEnemies.Logger.LogDebug("Test - Heal");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.Heal(25f);
			}

			// Debug key: Test Player Heal Over Time
			if (GlobalInputHandler.GetKeyUp(debugKeys[6]))
			{
				MortalEnemies.Logger.LogDebug("Test - Heal Over Time");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.HealOverTime(5f, 5f);
			}

			// Debug key: Kill all bots
			if (GlobalInputHandler.GetKeyUp(debugKeys[7]))
			{
				MortalEnemies.Logger.LogDebug("Test - Kill all Bots");
				MortalEnemies.Logger.LogDebug($"  Killing {BotMortalities.Count} bots");
				foreach (Mortality tempMortality in MortalSingleton.Instance.BotMortalities)
				{
					MortalEnemies.Logger.LogDebug($"  └Killing {tempMortality.gameObject.name}, which has {tempMortality.Health} health");
					tempMortality.Damage(tempMortality.MaxHealth);
				}
			}

			// Debug key: Revive all bots
			if (GlobalInputHandler.GetKeyUp(debugKeys[8]))
			{
				MortalEnemies.Logger.LogDebug("Test - Revive all Bots");
				MortalEnemies.Logger.LogDebug($"  Reviving {BotMortalities.Count} bots");
				foreach (Mortality tempMortality in MortalSingleton.Instance.BotMortalities)
				{
					MortalEnemies.Logger.LogDebug($"  └Reviving {tempMortality.gameObject.name}, which has {tempMortality.Health} health");
					tempMortality.Revive();
				}
			}

			// Debug key: Toggle screen logger
			if (GlobalInputHandler.GetKeyUp(debugKeys[9]))
			{
				MortalEnemies.Logger.LogDebug("Test - Toggling on-screen log");
				if (ScreenLogListener.Instance is null)
				{
					MortalEnemies.Logger.LogDebug("Could not find a ScreenLogListener to toggle");
					return;
				}
				ScreenLogListener.Instance.outputQueue = !ScreenLogListener.Instance.outputQueue;
				ScreenLogListener.Instance.RepopulateText();
			}

			// Debug key: Toggle screen logger
			if (GlobalInputHandler.GetKeyUp(debugKeys[2]))
			{
				MortalEnemies.Logger.LogDebug("Test - Teleport all living bots");
				if (BotHandler.instance is null)
				{
					MortalEnemies.Logger.LogDebug("BotHandler not instantiated");
					return;
				}

				foreach (Bot tempBot in BotHandler.instance.bots)
				{
					tempBot.Teleport(Level.currentLevel.GetClosestHiddenPoint(PlayerHandler.instance.GetRandomPlayerAlive().Center()).transform.position);
					MortalEnemies.Logger.LogDebug($"Teleported {tempBot.gameObject.name}");
				}


			}
		}
	}
}
