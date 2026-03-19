using UnityEngine;
using UnityEditor;
using System.Collections;

public class ChangeColor : EditorWindow
{

    GameObject target;
    MeshRenderer[] targets;
    Color gap;
    bool selectChildren = false;
    bool angleRotate = false;
    int axisValue;

    [MenuItem("ZOIT/ChangeColor")]
    static void Init()
    {
        ChangeColor window = (ChangeColor)EditorWindow.GetWindow(typeof(ChangeColor));
    }
	 
    void OnGUI()
    {
        //selectChildren = EditorGUILayout.Toggle("SelectChildren", selectChildren);
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject));
        gap = EditorGUILayout.ColorField("Color", gap);
        if (GUILayout.Button("Active"))
        {
            if (target != null)
            {
                targets = target.GetComponentsInChildren<MeshRenderer>();
                //foreach (MeshRenderer pos in targets)
				for( int k = 0 ; k < targets.Length; k++)
                {
                    for (int i = 0; i < targets[k].sharedMaterials.Length; i++)
                    {
                        targets[k].sharedMaterials[i].color = gap;
                    }
                }
            }
        }
    }
}
