using MyceliumNetworking;
using System.Collections.Generic;
using UnityEngine;

namespace MortalEnemies
{
	// Provides a globally accessible cache for Mortality components and settings
	public class MortalSingleton : MonoBehaviour
	{
		// Singleton pattern
		private static MortalSingleton? _instance;
		public static MortalSingleton Instance
		{
			get
			{
				// Ensure instance exists before returning reference
				if (_instance is null) CreateMortalSingleton();
				return _instance!;
			}
		}

		// VARIABLES
		private uint currentTick;
		public uint CurrentTick
		{
			get { return currentTick; }
			private set { }
		}

		private HashSet<Mortality> _mortalities = new();
		private HashSet<Mortality> _botMortalities = new();
		public HashSet<Mortality> Mortalities
		{
			get { return _mortalities; }
			internal set { _mortalities = value; }
		}
		public HashSet<Mortality> BotMortalities
		{
			get { return _botMortalities; }
			internal set { _botMortalities = value; }
		}

		// METHODS
		public static void CreateMortalSingleton()
		{
			_instance = GameObject.FindFirstObjectByType<MortalSingleton>();
			if (_instance is not null) return;

			GameObject tempObject = new GameObject("MortalSingleton", typeof(MortalSingleton));
			DontDestroyOnLoad(tempObject);
		}

		// EVENTS
		void Awake()
		{
			if (_instance == null) _instance = this; // Set _instance following singleton pattern
			else if (_instance != this) // Error state - singleton already exists
			{
				MortalEnemies.Logger.LogWarning("MortalSingleton already exists, destroying new duplicate");
				Destroy(this);
				return;
			}

			// Probably unnecessary, but rebuilds the hashtable state if the singleton is destroyed and recreated
			foreach (Mortality tempMort in FindObjectsOfType<Mortality>()) _mortalities.Add(tempMort);
			foreach (Mortality tempMort in FindObjectsOfType<Mortality_Bot>()) _botMortalities.Add(tempMort);
		}

		void FixedUpdate()
		{
			currentTick++; // should take around 2.7 years to overflow
		}
	}
}
