using UnityEngine;
using System.Collections;

public class renderQueue : MonoBehaviour {
	
	public int renderOrder = -10;
	// Use this for initialization
	void Start () {
	
		
	}
	
	// Update is called once per frame
	void Update () {
	GetComponent<Renderer>().material.renderQueue = renderOrder; // Unity6 Migration
	}
}
