using System.Collections.Generic;
using UnityEngine;

namespace MortalEnemies
{
	public class MortalSingleton : MonoBehaviour
	{
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
			// Debug key: Num1 - Test Player Damage
			if (Input.GetKeyUp(KeyCode.Keypad1))
			{
				MortalEnemies.Logger.LogDebug("Test - Player Damage");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.Damage(25f);
			}

			// Debug key: Num2 - Test Player Damage Over Time
			if (Input.GetKeyUp(KeyCode.Keypad2))
			{
				MortalEnemies.Logger.LogDebug("Test - Player Damage Over Time");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.DamageOverTime(5f, 5f);
			}

			// Debug key: Num3 - Test Player Heal
			if (Input.GetKeyUp(KeyCode.Keypad3))
			{
				MortalEnemies.Logger.LogDebug("Test - Heal");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.Heal(25f);
			}

			// Debug key: Num4 - Test Player Heal Over Time
			if (Input.GetKeyUp(KeyCode.Keypad4))
			{
				MortalEnemies.Logger.LogDebug("Test - Heal Over Time");
				Mortality tempMortality = Player.localPlayer.gameObject.GetComponent<Mortality>();
				if (tempMortality == null) MortalEnemies.Logger.LogDebug("Could not access mortality component");

				tempMortality?.HealOverTime(5f, 5f);
			}

			// Debug key: Num5 - Kill all bots
			if (Input.GetKeyUp(KeyCode.Keypad5))
			{
				MortalEnemies.Logger.LogDebug("Test - Kill all Bots");
				MortalEnemies.Logger.LogDebug($"  Killing {BotMortalities.Count} bots");
				foreach (Mortality tempMortality in MortalSingleton.Instance.BotMortalities)
				{
					MortalEnemies.Logger.LogDebug($"  └Killing {tempMortality.gameObject.name}, which has {tempMortality.Health} health");
					tempMortality.Damage(tempMortality.MaxHealth);
				}
			}

			// Debug key: Num6 - Revive all bots
			if (Input.GetKeyUp(KeyCode.Keypad6))
			{
				MortalEnemies.Logger.LogDebug("Test - Revive all Bots");
				MortalEnemies.Logger.LogDebug($"  Reviving {BotMortalities.Count} bots");
				foreach (Mortality tempMortality in MortalSingleton.Instance.BotMortalities)
				{
					MortalEnemies.Logger.LogDebug($"  └Reviving {tempMortality.gameObject.name}, which has {tempMortality.Health} health");
					tempMortality.Revive();
				}
			}
		}
	}
}
