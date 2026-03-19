using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

[CustomEditor(typeof(WaypoinEditor))]
public class WaypointEditorEditor : Editor {

    private static bool m_editMode = false;
    private static string m_preName = "wp";
    private static string m_folderName = "wps";
    private static int m_speed = 100;
    private GameObject m_container;
    public GameObject waypointFolder;
    public bool m_batchCreating = false;
    private bool m_lastFrameBatchCreating = false;
    public List<GameObject> m_ways;

    void OnSceneGUI()
    {
        if (m_editMode)
        {
            if (Event.current.type == EventType.MouseDown)
            {

                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;

                //2011-04-11 cse -B
                if (m_container == null)
                {
                    m_container = GameObject.Find(m_folderName);
                    //Debug.LogError("No container found. Place waypoints in scenes directly after pressing the Waypoint Editor button.");
                    //m_editMode = false;
                    //Repaint();
                }
                //2011-04-11 cse -E

                if (m_editMode) //2011-04-11 cse
                {               //2011-04-11 cse

                    if (Physics.Raycast(ray, out hit))
                    {
                        int counter = 1;
                        string fullPreName;
                        fullPreName = "/" + m_folderName + "/" + m_preName + "_";
                        while (GameObject.Find(fullPreName + counter.ToString()) != null)
                        {
                            counter++;
                        }

                        Undo.RegisterSceneUndo("Create new Waypoint");
                        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/WaypointScript/point.prefab", typeof(GameObject)) as GameObject;//"Assets/AIDriverToolkit/Prefabs/Waypoint.prefab", typeof(GameObject)) as GameObject;
                        GameObject waypoint = Instantiate(prefab) as GameObject;
                        Vector3 myPosition;
                        myPosition = hit.point;
                        myPosition.y = (float)myPosition.y + (float)(waypoint.transform.localScale.y / 2);

                        waypoint.transform.position = myPosition;
                        waypoint.name = m_preName + "_" + counter.ToString();
                        waypoint.transform.parent = m_container.transform;

                        WaypoinEditor script = (WaypoinEditor)target;
                        script.ways.Add(waypoint);
                        
                        //AIWaypoint aiwaypointScript = waypoint.GetComponent("AIWaypoint") as AIWaypoint;
                        //aiwaypointScript.speed = m_speed;
                        EditorUtility.SetDirty(waypoint);

                        //rotate last WP 
                        GameObject lastWP = GameObject.Find(fullPreName + (counter - 1).ToString());
                        if (lastWP != null)
                        {
                            lastWP.transform.LookAt(waypoint.transform);
                            EditorUtility.SetDirty(lastWP);
                        }
                    }

                    m_editMode = false;

                }//2011-04-11 cse 
            }
        }
    }

    bool waylist;
    public override void OnInspectorGUI()
    {

        WaypoinEditor script = (WaypoinEditor)target;

        script.folderName = EditorGUILayout.TextField("WP Parent", script.name);
        script.preName = EditorGUILayout.TextField("WP Prefix", script.preName);
        script.speed = EditorGUILayout.IntField("Speed", script.speed);
        script.batchCreating = EditorGUILayout.Toggle("Batch Creating", script.batchCreating);        
        
        script.show = EditorGUILayout.Toggle("Show", script.show);

        script.loopStartNum = EditorGUILayout.IntField("loop Start Num", script.loopStartNum);

        foreach (GameObject w in script.ways.ToArray())
        {
            w.GetComponent<Renderer>().enabled = script.show;
        }


        m_preName = script.preName;
        m_folderName = script.folderName;
        m_speed = script.speed;
        m_batchCreating = script.batchCreating;
        if (script.ways != null) m_ways = script.ways;

        if (m_lastFrameBatchCreating == true && m_batchCreating == false)
        {
            m_editMode = false;
        }

        if (m_editMode)
        {
            if (GUILayout.Button("Right Click in Scene View"))
            {
                m_editMode = false;
                script.batchCreating = false;
            }
        }
        else
        {
            if (GUILayout.Button("Press for new Waypoint") || m_batchCreating)
            {
                m_editMode = true;

                m_container = GameObject.Find(m_folderName);
                if (m_container == null)
                {
                    waypointFolder = new GameObject();
                    waypointFolder.name = m_folderName;
                    m_container = waypointFolder;
                }
            }

        }

        if (script.ways.ToArray().Length != 0)
        {
            if (GUILayout.Button("Remove Ways"))
            {
                foreach (GameObject obj in script.ways.ToArray())
                {
                    DestroyImmediate(obj);
                }
                script.ways.Clear();
                Debug.Log("Remove Ways");
            }
        }

        if (GUILayout.Button("Rebuild Array"))
        {
            script.ways.Clear();
            Transform[] a = script.transform.GetComponentsInChildren<Transform>();
            for (int i = 1; i < a.Length; i++)
            {
                script.ways.Add(a[i].gameObject);
                a[i].gameObject.name = m_preName + "_" + i.ToString();
            }
        }

        m_lastFrameBatchCreating = m_batchCreating;

        waylist = EditorGUILayout.Foldout(waylist, "WayList");
        if (waylist)
        {
            if (script.ways.ToArray().Length != 0)
            {
                foreach (GameObject obj in script.ways.ToArray())
                {
                    GameObject w = (GameObject)EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject));
                }
            }
        }
    }

}
