using UnityEngine;
using System.Collections;

public class Cycle_Smoke : MonoBehaviour {

    private Cycle_Control _control;

    private WheelCollider front;
    private WheelCollider rear;

    private GameObject smoke;
    private GameObject gravel;
    private GameObject water;
    private GameObject mud;

    // Unity6: ParticleEmitter removed — replaced by ParticleSystem (usage already commented out)
    // private ParticleEmitter particle;

	// Use this for initialization
	void Start () {
        _control = GetComponent<Cycle_Control>();
        WheelCollider[] all = transform.GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider pos in all)
        {
            if (pos.name == "Front")
            {
                front = pos;
            }
            else if (pos.name == "Rear")
            {
                rear = pos;
            }
        }
        //particle = transform.Find("Particle line").GetComponent<ParticleEmitter>();
        //particle.emit = false;
        smoke = (GameObject)Resources.Load("dust");
        gravel = (GameObject)Resources.Load("particles/particle_dirt");
        water = (GameObject)Resources.Load("particles/water_splash_cont");
        mud = (GameObject)Resources.Load("particles/water_splash_cont_med");
	}
	
	// Update is called once per frame
	void Update () {
        WheelHit hit_F;
        WheelHit hit_B;
        front.GetGroundHit(out hit_F);
        rear.GetGroundHit(out hit_B);

        if (_control.environment == Cycle_Control.Environment.Normal)
        {
            if (Mathf.Abs(hit_F.sidewaysSlip) > 0.5f && _control.moveValue.realSpeed > 5)
            {
                if (smoke)
                    Instantiate(smoke, hit_F.point, Quaternion.identity);
            }
            if (Mathf.Abs(hit_B.sidewaysSlip) > 0.5f && _control.moveValue.realSpeed > 5)
            {
                if (smoke)
                {
                    Instantiate(smoke, hit_B.point, Quaternion.identity);
                }
            }
        }
        else if (_control.environment == Cycle_Control.Environment.Mud)
        {
            if (mud && _control.moveValue.realSpeed > 5)
            {
                Instantiate(mud, hit_F.point, Quaternion.identity);
                Instantiate(mud, hit_B.point, Quaternion.identity);
            }
        }
        else if (_control.environment == Cycle_Control.Environment.Water)
        {
            if (water && _control.moveValue.realSpeed > 5)
            {
                Instantiate(water, hit_F.point, Quaternion.identity);
                Instantiate(water, hit_B.point, Quaternion.identity);
            }
        }
        else if (_control.environment == Cycle_Control.Environment.Gravel)
        {
            if (Mathf.Abs(hit_F.sidewaysSlip) > 0.5f && _control.moveValue.realSpeed > 0)
            {
                if (gravel)
                    Instantiate(gravel, hit_F.point, Quaternion.identity);
            }
            if (Mathf.Abs(hit_B.sidewaysSlip) > 0.5f && _control.moveValue.realSpeed > 0)
            {
                if (gravel)
                {
                    Instantiate(gravel, hit_B.point, Quaternion.identity);
                }
            }
        }

        //if (_control.moveValue.realSpeed > 40)
        //{
        //    particle.emit = true;
        //}
        //else
        //{
        //    particle.emit = false;
        //}
	}
}
