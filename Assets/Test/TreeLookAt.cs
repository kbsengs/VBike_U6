using UnityEngine;
using System.Collections;

public class TreeLookAt : MonoBehaviour {
	
	public Transform target;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(target);
		transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
	}
}
