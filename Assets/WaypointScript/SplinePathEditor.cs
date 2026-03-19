using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplinePathEditor : MonoBehaviour {

    public Transform _Way;

    public int steps = 5;
    public bool loop = true;
    public Color color = Color.white;
    //[HideInInspector]
    public List<Transform> path;
    private Vector3[] pathPositions;
    //[HideInInspector]
    public List<Vector3> sequence;
    private bool isLoaded = false;

    //public bool active = true;
    public bool show = false;
    private string m_waypointPreName = "MyWaypoint";
    private string m_waypointFolder = "MyWaypoints";
    public int loopStartNum;

    //private Transform parent;

    public List<GameObject> ways;

    public void Init()
    {
        GetWaypointNames();
        FillPath();
        FillSequence();
        CreateNewWaypoints();
    }

    public void CreateNewWaypoints()
    {

        int counter = 0;

        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Waypoint/point.prefab", typeof(GameObject)) as GameObject;

        foreach (Vector3 point in sequence)
        {

            counter++;

            //den letzten erzeugen wir nicht, da dieses die gleich Position hat wie der erste
            if (counter < sequence.Count || !loop)
            {
                GameObject waypoint = Instantiate(prefab) as GameObject;
                waypoint.transform.position = point;
                waypoint.name = m_waypointPreName + "S_" + counter.ToString();
                waypoint.transform.parent = transform;
                //AIWaypoint aiwaypointScript = waypoint.GetComponent("AIWaypoint") as AIWaypoint;	

                CopyParameters(ref waypoint, counter);
                ways.Add(waypoint);
            }
        }
        GameObject[] setWays = ways.ToArray();
        for (int i = 0; i < setWays.Length; i++)
        {
            if (i < setWays.Length - 1)
            {
                setWays[i].transform.LookAt(setWays[i + 1].transform);
            }
            else
            {
                setWays[i].transform.LookAt(setWays[0].transform);
            }
        }
    }

    public void CopyParameters(ref GameObject waypoint, int newIndex)
    {

        float fltOldIndex = newIndex / (steps + 1);

        int intOldIndex;

        int modIndex = newIndex % (steps + 1);

        if (modIndex == 0)
        {
            intOldIndex = newIndex / (steps + 1);
        }
        else
        {
            intOldIndex = 1 + (newIndex / (steps + 1));
        }


        //AIWaypoint oldAiWaypointScript = path[intOldIndex - 1].GetComponent("AIWaypoint") as AIWaypoint;

        //AIWaypoint aiWaypointScript = waypoint.GetComponent("AIWaypoint") as AIWaypoint;

        //aiWaypointScript.speed = oldAiWaypointScript.speed;
        //aiWaypointScript.useTrigger = oldAiWaypointScript.useTrigger;

        //if (aiWaypointScript.useTrigger)
        //{
        //    BoxCollider bc = waypoint.AddComponent<BoxCollider>();	
        //    bc.isTrigger = true;
        //    //waypoint.layer = path[intOldIndex - 1].gameObject.layer; Die automatische Zuweisung geschieht erst spaeter
        //    waypoint.layer = 2;
        //}

        waypoint.transform.localScale = path[intOldIndex - 1].localScale;
        waypoint.tag = path[intOldIndex - 1].gameObject.tag;

    }

    public void FillPath()
    {
        bool found = true;
        int counter = 1;

        path.Clear();

        while (found)
        {
            GameObject go;
            string currentName;
            currentName = "/" + m_waypointFolder + "/" + m_waypointPreName + counter.ToString();
            go = GameObject.Find(currentName);

            if (go != null)
            {
                path.Add(go.transform);
                counter++;
            }
            else
            {
                found = false;
            }

        }
    }

    public void FillSequence()
    {

        sequence = SplineCalculation.NewCatmullRom(path, steps, loop);

    }

    public void GetWaypointNames()
    {
        WaypoinEditor waypointEditor;

        waypointEditor = _Way.GetComponent("WaypoinEditor") as WaypoinEditor;
        //waypointEditor = 
        if (waypointEditor != null)
        {
            m_waypointPreName = waypointEditor.preName + "_";
            m_waypointFolder = waypointEditor.folderName;
            //Debug.Log("Null Way. Set Way Transform.");
        }
    }
}
