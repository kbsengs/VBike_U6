using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {
    public float xSpeed = 50;
    public float ySpeed = 50;

    public float keyboardSpeed = 1;
	// Use this for initialization
	void Start () {
	    
	}

    private float x = 0.0f;
    private float y = 0.0f;
	// Update is called once per frame
	void LateUpdate () {
        transform.Translate(new Vector3(Input.GetAxis("Horizontal") * keyboardSpeed, 0, Input.GetAxis("Vertical") * keyboardSpeed));
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y += Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        transform.rotation = Quaternion.Euler(-y, x, 0);
	}
}
