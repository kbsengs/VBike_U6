using UnityEngine;
using System.Collections;

public class TestMove : MonoBehaviour {

    public float speed = 10;
    SplinePathWaypoints way;
    public int wayNum;
    public Transform target;

    public WheelCollider lWheel;
    public WheelCollider rWheel;

    public WheelCollider lrWheel;
    public WheelCollider rrWheel;

    public float maxAngle = 30;
	// Use this for initialization
	void Start () {
        way = FindObjectOfType(typeof(SplinePathWaypoints)) as SplinePathWaypoints;
        //rigidbody.centerOfMass = transform.Find("CenterOfMass").position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        FindWay();

        if (target == null)
            return;
        
        //Vector3 targetVelocity = Vector3.forward;
        //targetVelocity = transform.TransformDirection(targetVelocity);
        //targetVelocity *= speed;

        //Vector3 velocity = rigidbody.velocity;
        //Vector3 velocityChange = (targetVelocity - velocity);
        //velocityChange.x = Mathf.Clamp(velocityChange.x, -speed, speed);
        //velocityChange.z = Mathf.Clamp(velocityChange.z, -speed, speed);
        //velocityChange.y = 0;
        //rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

        //Quaternion rotation = Quaternion.LookRotation(new Vector3(target.position.x, 0, target.position.z) - new Vector3(transform.position.x, 0, transform.position.z));
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 2.0f * Time.fixedDeltaTime);

        AiSteering();

        lrWheel.motorTorque = speed;
        rrWheel.motorTorque = speed;

        lWheel.steerAngle = aiSteerAngle;
        rWheel.steerAngle = aiSteerAngle;
	}

    float targetAngle;
    float currentAngle;
    float steeringSpeed = 100;
    public float calcMaxSpeed = 200.0f;

    float steerAngle = 20.0f;
    float hsSteerAngle = 5.0f;
    float aiSteerAngle;

    void AiSteering()
    {
        Vector3 wantTarget = target.position;
        Vector3 moveDirection = wantTarget - transform.position;
        Vector3 localTarget = transform.InverseTransformPoint(wantTarget);

        targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        //iaSteerAngle = Mathf.Clamp(targetAngle, -1, 1);


        if (currentAngle < targetAngle)
        {
            currentAngle = currentAngle + (Time.deltaTime * steeringSpeed);
            if (currentAngle > targetAngle)
            {
                currentAngle = targetAngle;
            }
        }
        else if (currentAngle > targetAngle)
        {
            currentAngle = currentAngle - (Time.deltaTime * steeringSpeed);
            if (currentAngle < targetAngle)
            {
                currentAngle = targetAngle;
            }
        }


        //je hoeher die Geschwindigkeit,  desto geringer der maximale Einschlagwinkel.
        float aiCalculationMaxSpeed = calcMaxSpeed;
        //float speedProcent = currentSpeed / maxSpeed;
        float speedProcent = speed / aiCalculationMaxSpeed;
        speedProcent = Mathf.Clamp(speedProcent, 0, 1);
        float speedControlledMaxSteerAngle;
        speedControlledMaxSteerAngle = steerAngle - ((steerAngle - hsSteerAngle) * speedProcent);


        aiSteerAngle = Mathf.Clamp(currentAngle, (-1) * speedControlledMaxSteerAngle, speedControlledMaxSteerAngle);

        //Debug.Log(aiSteerAngle);
    }

    void FindWay()
    {
        if (target == null)
        {
            if (way.ways.ToArray().Length > 0)
            {
                target = way.ways.ToArray()[wayNum].transform;
            }
        }
        else 
        {
            if (Vector3.Distance(transform.position, target.position) < 10)
            {
                wayNum++;
                if (wayNum >= way.ways.ToArray().Length)
                {
                    wayNum = 0;
                }
                target = way.ways.ToArray()[wayNum].transform;
            }
        }
    }
}
