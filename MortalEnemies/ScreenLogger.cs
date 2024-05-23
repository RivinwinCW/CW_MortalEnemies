using BepInEx.Logging;
using System.Collections;
using UnityEngine;

namespace MortalEnemies
{
	public class ScreenLogListener
	{
		private const int logLength = 52;

		public static ScreenLogListener? Instance; // this is bad code, make it a true singleton or drop the naming scheme
		Queue logQueue = new Queue(logLength);
		private UnityEngine.UI.Text? MyText;
		public GameObject gameObject;
		public bool outputQueue;

		public ScreenLogListener()
		{
			Instance = this; // this is bad code, make it a true singleton or drop the naming scheme

			gameObject = new("ScreenLogger");

			GameObject newParentObject = TransitionHandler.Instance.transform.Find("Canvas").gameObject;
			if (newParentObject == null)
			{
				MortalEnemies.Logger.LogError($"Parent Canvas not found");
				return;
			}
			gameObject.transform.SetParent(newParentObject.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = new Vector3(1, 1, 1);
			RectTransform test = gameObject.AddComponent<RectTransform>();
			test.anchorMin = new Vector2(0.75f, 0.05f); //0.6f, 0.6f
			test.anchorMax = new Vector2(0.943f, 0.95f); //0.943f, 0.89f

			CanvasRenderer canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
			canvasRenderer.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

			var canvasGroup = gameObject.AddComponent<CanvasGroup>();
			canvasGroup.blocksRaycasts = false;

			// Load font
			//Font font = Font.CreateDynamicFontFromOSFont("Noto Sans", 16);
			Font font = Font.CreateDynamicFontFromOSFont("Arial", 16);

			// Add text block component
			MyText = gameObject.AddComponent<UnityEngine.UI.Text>();
			if (MyText is not null)
			{
				MyText.font = font;
				MyText.fontSize = 16;
				MyText.supportRichText = false;
			}
			else
			{
				MortalEnemies.Logger.LogError($"Text component not created");
				return;
			}
			MortalEnemies.Logger.LogEvent += Log_LogEvent;
		}

		private void Log_LogEvent(object sender, LogEventArgs logEvent)
		{
			// Could add filtering and formatting here
			HandleLog(logEvent.Data.ToString().Replace("\n", ""));
		}

		public void HandleLog(string message)
		{
			if (logQueue.Count > logLength) logQueue.Dequeue(); // Make room if we've filled all lines
			logQueue.Enqueue(message); // Add the new message to the queue

			if (MyText == null) return; // Sanity check in case the listener is externally subscribed to but not instantiated properly

			
		}

		// Clear and repopulate the Text component with the contents of the queue
		public void RepopulateText()
		{
			MyText.text = "";
			if (outputQueue) foreach (string log in logQueue) MyText.text += $">  {log}\n";
		}
	}
}