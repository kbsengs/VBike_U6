using UnityEngine;
using System.Collections;

public class MemoryClear : MonoBehaviour {

	void Awake()
	{
		QualitySettings.currentLevel = QualityLevel.Fantastic;
		Debug.Log (QualitySettings.currentLevel);
	}

	// Update is called once per frame
	void Update () {
		if (Time.frameCount % 30 == 0) {
			System.GC.Collect();
				}
	}
}
