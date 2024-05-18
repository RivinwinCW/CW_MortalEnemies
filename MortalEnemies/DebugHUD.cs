using UnityEngine;

namespace MortalEnemies
{
	internal class MortalityHUD : MonoBehaviour
	{
		private static MortalityHUD? _instance;
		public static MortalityHUD Instance
		{
			get
			{
				if (_instance == null) CreateMortalityHUD();
				return _instance;
			}
		}

		public static void CreateMortalityHUD()
		{
			_instance = GameObject.FindFirstObjectByType<MortalityHUD>();
			if (_instance != null) return;

			GameObject newObject = new GameObject("MortalityHUD", typeof(MortalityHUD));
			DontDestroyOnLoad(newObject);
		}

		void Start()
		{
			Transform newParent = TransitionHandler.Instance.transform.Find("Canvas");
			if (newParent == null)
			{
				MortalEnemies.Logger.LogError($"Parent Canvas not found");
				return;
			}
			gameObject.transform.SetParent(newParent);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = new Vector3(1, 1, 1);

			CanvasRenderer canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
			canvasRenderer.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

			var canvasGroup = gameObject.AddComponent<CanvasGroup>();
			canvasGroup.blocksRaycasts = false;

			Font font = Font.CreateDynamicFontFromOSFont("Arial", 16);

		}
	}

	internal class MortalityHUDSource : MonoBehaviour
	{
		// Debug
		private static Camera? mainCamera;
		private static bool cameraFound;

		// References
		private Mortality? targetMortality;
		private Transform? headBone;

		void Awake()
		{
			mainCamera = Camera.main;
		}

		public void SetMortality(Mortality newTarget)
		{
			targetMortality = newTarget;
			if (targetMortality != null) MortalEnemies.Logger.LogDebug("Set new Mortality Target for DebugHUD");
			else
			{
				MortalEnemies.Logger.LogDebug("Mortality newTarget is null in SetMortality()");
				return;
			}

			Player tempPlayer = targetMortality.gameObject.GetComponentInChildren<Player>();
			if (tempPlayer is null) MortalEnemies.Logger.LogDebug("Player not found in parent gameobject");
			else
			{
				headBone = tempPlayer.refs.ragdoll.GetBodypart(BodypartType.Head).rig.transform;
				if (headBone is not null) transform.SetParent(headBone);
				else MortalEnemies.Logger.LogDebug("Player does not contain a head bone");
			}
			
			gameObject.transform.localPosition = new Vector3(0, 1, 0); // offset above the head, might need to offset during render in order to ignore parent rotation
			gameObject.AddComponent<Canvas>();
		}

		void LateUpdate()
		{
			if (mainCamera is null) MortalEnemies.Logger.LogDebug("MainCamera is Null!!!!!!!!!!!");
			if (headBone is null || mainCamera is null) return; // Sanity check, for instance the same frame that the Mortality object or camera gets destroyed

			if (!cameraFound)
			{
				MortalEnemies.Logger.LogDebug("Debug HUD - Main camera found");
				cameraFound = true;
			}

			Vector3 screenPos = Camera.main.WorldToScreenPoint(this.transform.position);
			//MortalityHUD
		}
	}
}
