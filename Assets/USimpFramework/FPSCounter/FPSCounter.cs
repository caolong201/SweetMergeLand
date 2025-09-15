using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractiveSeven
{
	public class FPSCounter : MonoBehaviour
	{
		public enum DeltaTimeType
		{
			Smooth,
			Unscaled
		}

		[Tooltip("Unscaled is more accurate, but jumpy, or if your game modifies Time.timeScale. Use Smooth for smoothDeltaTime.")]
		public DeltaTimeType DeltaType = DeltaTimeType.Smooth;

		GUIStyle style;

		Dictionary<int, string> cachedNumberStringDic = new();

		int[] frameRateSamples;
		int cacheNumbersAmount = 300;
		int averageFromAmount = 30;
		int averageCounter;
		int currentAveraged;
		
		void Awake()
		{
			// Cache strings and create array
			for (int i = 0; i < cacheNumbersAmount; i++)
			{
				cachedNumberStringDic[i] = i.ToString();
			}

			frameRateSamples = new int[averageFromAmount];

		}
		
		void Start()
		{
			GUI.depth = 2;
			style = new() { fontSize = 30, fontStyle = FontStyle.Bold };
		}

		void Update()
		{
			// Sample
			var currentFrame = (int)Mathf.Round(1f / DeltaType switch
			{
				DeltaTimeType.Smooth => Time.smoothDeltaTime,
				DeltaTimeType.Unscaled => Time.unscaledDeltaTime,
				_ => Time.unscaledDeltaTime
			});
			frameRateSamples[averageCounter] = currentFrame;


			// Average
			var average = 0f;

			foreach (var frameRate in frameRateSamples)
			{
				average += frameRate;
			}

			currentAveraged = (int)Mathf.Round(average / averageFromAmount);
			averageCounter = (averageCounter + 1) % averageFromAmount;

		}

		void OnGUI()
		{
			string fpsStr = "";
			if (currentAveraged >= 0 && currentAveraged < cacheNumbersAmount)
			{
				fpsStr = cachedNumberStringDic[currentAveraged];
			}
			else if (currentAveraged >= cacheNumbersAmount)
			{
				fpsStr = $"> {cacheNumbersAmount}";
			}
			else if (currentAveraged < 0)
			{
				fpsStr = "< 0";
			}

			GUI.Label(new Rect(Screen.width - 150, 20, 100, 25), "FPS: " + fpsStr, style);
		}
	}
}
