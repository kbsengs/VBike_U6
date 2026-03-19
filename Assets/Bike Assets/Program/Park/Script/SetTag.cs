using UnityEngine;
using System.Collections;

public class SetTag : MonoBehaviour {

    public string name;
	// Use this for initialization
	void Start () {
        Transform[] target = GetComponentsInChildren<Transform>();
        foreach (Transform a in target)
        {
            a.tag = name;
        }
	}
	
	// Update is called once per frame
	
}
