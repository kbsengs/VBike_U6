using UnityEngine;
using System.Collections;

public class MinimapSetTexture : MonoBehaviour {
    Renderer minimap;
	// Use this for initialization
	void Start () {
        minimap = transform.Find("mini map_box").GetComponent<Renderer>(); // Unity6 Migration
	}

    void Update()
    {
        MinimapTexture target = FindObjectOfType(typeof(MinimapTexture)) as MinimapTexture;
        minimap.material.SetTexture("_MainTex", target.GetComponent<Camera>().targetTexture);
    }
}
