using UnityEngine;
using System.Collections;

public class CycleCam : MonoBehaviour
{
    // The target we are following
    //public Transform[] targets;
    public Transform target;
     //The distance in the x-z plane to the target
    public float distance = 10.0f;
    // the height we want the camera to be above the target
    public float height = 5.0f;
     //How much we 
    public float heightDamping = 3.0f;
    public float rotationDamping = 0.1f;
    public bool dead = false;

    public enum State
    {
        SmoothLook, MouseOrbit
    }
    public State state = State.MouseOrbit;

    //private MapData map;

    private Vector2[] cameraPosition = new Vector2[] { new Vector2(2.0f, 0.1f), new Vector2(3.5f, 0.5f), new Vector2(4.0f, 0.5f) };
    //public Vector2[] cameraPointPos = new Vector2[] { new Vector2(1.4f, 0.4f), new Vector2(1.0f, 2.5f), new Vector2(1.3f, 1.0f)};

    public void SetTarget(Transform t, int camera)
    {
        target = t;
        dead = false;
        CameraPosChange(cameraPosition[camera]);//, cameraPointPos[DataInfo.camera]);
    }

    public void SetTargetDead(Transform t)
    {
        target = t;
        dead = true;
        CameraPosChange(cameraPosition[2]);//, cameraPointPos[DataInfo.camera]);
    }

    void Start()
    {
		//state = State.MouseOrbit;
        CameraPosChange(cameraPosition[1]);
    }

    void CameraPosChange(Vector2 pos1)//, Vector2 pos2)
    {    
        height = pos1.y;
        distance = pos1.x;
        if (!dead)
        {
            heightDamping = 3.0f;
            rotationDamping = 3.0f;
        }
        else
        {
            heightDamping = 1.0f;
            rotationDamping = 0.5f;
        }
    }

    void LateUpdate () {
        if (state == State.MouseOrbit)
            Orbit();
        else SmoothLookAt(); 
    }

    void SmoothLookAt()
    {
        if (target)
        {
            float wantedRotationAngle = target.eulerAngles.y;
            float wantedHeight = target.position.y + height;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);


            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = target.position;
            transform.position -= currentRotation * Vector3.forward * distance;

            if (dead)
            {
            // moonpro
                float minheight = 1.0f;
                Vector3 pos = new Vector3(transform.position.x, currentHeight, transform.position.z);
                RaycastHit hit;
                bool b = Physics.Raycast(pos, new Vector3(0, 1, 0), out hit, 100);

                if (b && hit.distance < 1 && hit.point.y < target.position.y + 1) currentHeight = hit.point.y + minheight;
                else
                {
                    Physics.Raycast(pos, new Vector3(0, -1, 0), out hit, 100);
                    if (hit.distance < minheight) currentHeight = hit.point.y + minheight;
                }
                transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
                transform.LookAt(target);
            }
            // Set the height of the camera

            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            //transform.position = target.TransformPoint(0, 0, 0.5f);
            //Debug.Log(target.localEulerAngles.y);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.Clamp(target.localEulerAngles.y, -60, 60), transform.localEulerAngles.z);
            // Always look at the target
            transform.LookAt(target);
        }
    }

    public float orbitDistance = 10.0f;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20;
    public float yMaxLimit = 80;

    private float x;
    private float y;
		
	float minDist = 2.0f;
	float maxDist = 20.0f;
	float wheelValue = 0.0f;
	
    void Orbit()
    {
        if (target)
        {
			float fwheel = Input.GetAxis("Mouse ScrollWheel") ;
			if (fwheel != 0)
			{
				wheelValue = fwheel;
			}
				
				orbitDistance -= ( wheelValue * 20 * Time.deltaTime);
				
				if (orbitDistance < minDist) orbitDistance = minDist; 
				if (orbitDistance > maxDist) orbitDistance = maxDist;		
			
			
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -orbitDistance) + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}