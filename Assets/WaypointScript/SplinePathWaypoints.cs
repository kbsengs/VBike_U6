using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplinePathWaypoints : SplinePath {
	
	public bool active = true;
	public bool show = false;
	private string m_waypointPreName = "MyWaypoint";
    private string m_waypointFolder = "MyWaypoints";
	
	private Transform parent;

    public List<GameObject> ways;
	
	protected override void Awake () {
		
		if(active)
			Init();
	}
	
	// Use this for initialization
	void Start () {
		
		if(show && active)
		{
			SetRenderer(true);
		}
			
	}
	
	protected virtual void OnDrawGizmos() {
	
		if (active && (!Application.isPlaying || show))
		{
			GetWaypointNames();
			FillPath();
			FillSequence();
			DrawGizmos();
		}
		
		if(!Application.isPlaying)
		{
			//SetDrawLineToNext();
		}
		        
	}
	
	void Init()
	{
		GetWaypointNames();
		FillPath();
		FillSequence();
		parent = GameObject.Find(m_waypointFolder).transform;
		CreateNewWaypoints();
		RenamePathObjects();
	}

	void CreateNewWaypoints()
	{
		
		int counter = 0;
		
		GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/point.prefab", typeof(GameObject)) as GameObject;
		
		foreach (Vector3 point in sequence)
		{
			
			counter ++;			
			
			//den letzten erzeugen wir nicht, da dieses die gleich Position hat wie der erste
			if (counter < sequence.Count || !loop)
			{
	            GameObject waypoint = Instantiate(prefab) as GameObject;                  
	            waypoint.transform.position = point;
	            waypoint.name = m_waypointPreName + counter.ToString();
	            waypoint.transform.parent = parent;
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
	
	void CopyParameters(ref GameObject waypoint, int newIndex)
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
			intOldIndex = 1 +(newIndex / (steps + 1));
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
	
	void RenamePathObjects()
	{
		foreach(Transform current in path)
		{
			
			current.gameObject.name = current.gameObject.name + "_original";
			
		}
	}
	
	void FillPath() 
    {				
        bool found=true;
        int counter=1;

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
	
	void GetWaypointNames()
    {
        WaypoinEditor waypointEditor;

        waypointEditor = GetComponent("WaypoinEditor") as WaypoinEditor;
        if (waypointEditor != null)
        {
            m_waypointPreName = waypointEditor.preName + "_";
            m_waypointFolder = waypointEditor.folderName;
        }
    }

	void SetRenderer(bool active)
	{
				
		bool found=true;
        int counter=1;

        path.Clear();
		
        while (found)
        {
			GameObject go;            			
			string currentName;
            currentName = "/" + m_waypointFolder + "/" + m_waypointPreName + counter.ToString();            
			go = GameObject.Find(currentName);
            
            if (go != null)
            {				                
				go.GetComponent<Renderer>().enabled = active;
                counter++;
            }
            else
            {
                found = false;               
            }
            
        }      
	
	}
	
	void SetDrawLineToNext()
	{
		if (active)
		{
			
		}
		bool found=true;
        int counter=1;
		
		path.Clear();
		
        while (found)
        {
			GameObject go;            			
			string currentName;
            currentName = "/" + m_waypointFolder + "/" + m_waypointPreName + counter.ToString();            
			go = GameObject.Find(currentName);
            
            if (go != null)
            {				                
				DrawLineToNext drawLineToNext = go.GetComponent<DrawLineToNext>() as DrawLineToNext;
				if (drawLineToNext !=null)
				{
					// Unity6 Migration: Component.active -> gameObject.SetActive()
					drawLineToNext.gameObject.SetActive(!active);
				}
				
            }
            else
            {
                found = false;               
            }
            
        }      
				
	}
}