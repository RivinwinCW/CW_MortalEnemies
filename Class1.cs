using BepInEx.Logging;
using System.Collections;
using UnityEngine;

namespace ConfirmQuitCW
{
	public class ScreenLogListener
	{
		public static ScreenLogListener? Instance;
		Queue logQueue = new Queue(10);
		private UnityEngine.UI.Text? MyText;
		public GameObject gameObject;

		public ScreenLogListener()
		{
			Instance = this;

			gameObject = new("ScreenLogger");

			GameObject newParentObject = TransitionHandler.Instance.transform.Find("Canvas").gameObject;
			if (newParentObject == null)
			{
				ConfirmQuitCW.Logger.LogError("Parent Canvas not found");
				return;
			}
			gameObject.transform.SetParent(newParentObject.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = new Vector3(1, 1, 1);
			RectTransform test = gameObject.AddComponent<RectTransform>();
			test.anchorMin = new Vector2(0.6f, 0.6f);
			test.anchorMax = new Vector2(0.943f, 0.89f);

			CanvasRenderer canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
			canvasRenderer.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

			var canvasGroup = gameObject.AddComponent<CanvasGroup>();
			canvasGroup.blocksRaycasts = false;

			Font font = Font.CreateDynamicFontFromOSFont("Noto Sans", 8);

			MyText = gameObject.AddComponent<UnityEngine.UI.Text>();
			if (MyText is not null)
			{
				MyText.font = font;
				MyText.fontSize = 24;
				MyText.supportRichText = false;
			}
			else
			{
				ConfirmQuitCW.Logger.LogError("Text component not created");
				return;
			}
			ConfirmQuitCW.Logger.LogEvent += Log_LogEvent;
		}

		private void Log_LogEvent(object sender, LogEventArgs logEvent)
		{
			// Could add filtering and formatting here
			HandleLog(logEvent.Data.ToString().Replace("\n", ""));
		}

		public void HandleLog(string message)
		{
			if (logQueue.Count == 14) logQueue.Dequeue(); // Make room if we've filled all lines
			logQueue.Enqueue(message); // Add the new message to the queue

			if (MyText == null) return; // Sanity check in case the listener is externally subscribed to but not instantiated properly

			// Clear and repopulate the Text component with the contents of the queue
			MyText.text = "";
			foreach (string log in logQueue) MyText.text += $"{log}\n";
		}
	}
}