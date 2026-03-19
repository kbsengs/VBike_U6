using UnityEngine;
using System.Collections;

public class CRollingStone2 : MonoBehaviour
{
    public Vector3 dir;
    public float power;
    public float starttime;
    public float againtime;
	public float mass;

    float time = 0;
    Vector3 pos;
	Vector3 rot;
    bool start = false;
    private Rigidbody _rb; // Unity6 Migration

// Use this for initialization

void Start ()
{
    //gameObject.AddComponent("NetworkView");
    _rb = GetComponent<Rigidbody>(); // Unity6 Migration
    if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>(); // Unity6 Migration
    gameObject.tag = "Falldown";
    pos = transform.position;
    rot = transform.eulerAngles;
    time = -starttime;
    _rb.mass = mass; // Unity6 Migration
    _rb.Sleep(); // Unity6 Migration
}


	// Update is called once per frame

void Update ()
    {
        time += Time.deltaTime;

        if (!start && time > starttime)
        {
            _rb.WakeUp(); // Unity6 Migration
            // Unity6 Migration: Network.isServer removed; always apply force in single-player
            _rb.AddForce(dir * power); // Unity6 Migration
            start = true;
        }

        if (time > againtime)
        {
			_rb.angularVelocity = Vector3.zero; // Unity6 Migration
            _rb.linearVelocity = Vector3.zero; // Unity6 Migration
            transform.position = pos;
            transform.eulerAngles = rot;
            //rb.Sleep();
            time = 0;
            // Unity6 Migration: Network.isServer removed; always apply force in single-player
            _rb.AddForce(dir * power); // Unity6 Migration
            //start = false;
        }
	}
}
