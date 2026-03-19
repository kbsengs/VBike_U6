using UnityEngine;
using System.Collections;

public class collidertest  :  MonoBehaviour 
{	
	public  GameObject fPrefab;
	void OnCollisionStay(Collision collision) 
	{
		foreach (ContactPoint contact in collision.contacts) 
		{
			//print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
			Debug.DrawRay(contact.point, contact.normal, Color.white);
			
			Quaternion rot = Quaternion.FromToRotation(-Vector3.up, contact.normal);
			Vector3 pos = contact.point;
			
			GameObject obj = Instantiate((GameObject)fPrefab, pos, rot) as GameObject;
			Destroy( obj, 1.0f );
		}
	}
		
}