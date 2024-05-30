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
				if (_instance is null) CreateMortalitySingleton();
				return _instance!;
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
			if (_instance is not null) return;

			GameObject tempObject = new GameObject("MortalSingleton", typeof(MortalSingleton));
			DontDestroyOnLoad(tempObject);
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
		}
	}
}
