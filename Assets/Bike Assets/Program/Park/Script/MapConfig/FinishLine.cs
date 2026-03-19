using UnityEngine;
using System.Collections;

public class FinishLine : MonoBehaviour {

    public int count = 0;
    public int pastCount = 0;
    //public Transform point;
    //public bool act = false;
    //public BMX_Server_Data _Data;

    public Transform target;

    //void Start()
    //{
    //    point = transform.Find("point");
    //}

    //public void Init()
    //{
    //    _Data = FindObjectOfType(typeof(BMX_Server_Data)) as BMX_Server_Data;
    //}

    //void FixedUpdate()
    //{
    //    if (act)
    //    {
    //        RaycastHit hit;
    //        //hits = Physics.RaycastAll(transform.position, transform.forward, 30.0F);
    //        //for (int i = 0; i < hits.Length; i++)
    //        //{
    //        //    if (hits[i].collider.tag == "Player")
    //        //    {
    //        //        target = hits[i].collider.transform;
    //        //    }
    //        //}
    //        if (Physics.Raycast(point.position, point.TransformDirection(Vector3.forward), out hit, 20))
    //        {
    //            if (hit.collider.tag == "Player" && count < 3 && hit.collider.transform != target)
    //            {
    //                _Data.ReplaySave(count);
    //                target = hit.collider.transform;
    //                hit.collider.gameObject.layer = 0;
    //                count++;
    //                Debug.Log(count);
    //            }
    //        }
    //        Debug.DrawRay(point.position, point.TransformDirection(Vector3.forward), Color.red);
    //    }
    //}
}
