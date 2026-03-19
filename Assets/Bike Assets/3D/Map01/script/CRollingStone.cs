using UnityEngine;
using System.Collections;

public class CRollingStone : MonoBehaviour
{
    public Vector3 dir;
    public float power;
    public float starttime;
    public float againtime;
	public float mass;

    float time = 0;
    Vector3 pos;
    bool start = false;
    private Rigidbody _rb; // Unity6 Migration

// Use this for initialization

void Start ()
{
    //gameObject.AddComponent("NetworkView");
    _rb = gameObject.GetComponent<Rigidbody>(); // Unity6 Migration
    if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>(); // Unity6 Migration
    gameObject.tag = "Falldown";
    pos = transform.position;
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
            _rb.Sleep(); // Unity6 Migration
            time = 0;
            start = false;
        }
	}
}
