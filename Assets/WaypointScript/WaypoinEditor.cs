using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WaypoinEditor : MonoBehaviour {

    public string folderName = "Waypoints";
    public string preName = "point";
    public int speed = 100;
    public Material waypointMaterial;
    public bool show;
    public bool batchCreating = false;
    public int loopStartNum = 0;

    public List<GameObject> ways;


}
