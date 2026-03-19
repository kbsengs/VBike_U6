using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(SplinePathEditor))]
public class SplinePathEditorEditor : Editor
{
    bool waylist;

    public override void OnInspectorGUI()
    {
        SplinePathEditor script = (SplinePathEditor)target;

        script._Way = (Transform)EditorGUILayout.ObjectField("Target Way", script._Way, typeof(Transform));
        script.steps = EditorGUILayout.IntField("Steps", script.steps);
        script.loop = EditorGUILayout.Toggle("Loop", script.loop);
        //script.active = EditorGUILayout.Toggle("Active", script.active);
        script.show = EditorGUILayout.Toggle("Show", script.show);
        script.loopStartNum = EditorGUILayout.IntField("Loop Start Way Number", script.loopStartNum);

        foreach (GameObject w in script.ways.ToArray())
        {
            w.GetComponent<Renderer>().enabled = script.show;
            w.GetComponent<DrawLineToNext>().show = script.show;
        }

        if (script.ways.ToArray().Length == 0)
        {
            if (GUILayout.Button("Set"))
            {   
                script.Init();
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
