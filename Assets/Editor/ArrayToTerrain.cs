using UnityEngine;
using UnityEditor;
using System.Collections;

public class ArrayToTerrain : EditorWindow {

    GameObject target;
    Transform[] targets;
    float gap = 0;
    bool selectChildren = false;
    bool angleRotate = false;
    int axisValue;
    Vector3 scale;

    [MenuItem("Custom/ArrayToTerrain")]
    static void Init()
    {
        ArrayToTerrain window = (ArrayToTerrain)EditorWindow.GetWindow(typeof(ArrayToTerrain));
    }

    void OnGUI()
    {
        selectChildren = EditorGUILayout.Toggle("SelectChildren", selectChildren);
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject));
        gap = EditorGUILayout.FloatField("Height", gap);
        angleRotate = EditorGUILayout.Toggle("AngleRotate", angleRotate);
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Axis", null);
        string[] axis = new string[] { "X-Axis", "Z-Axis", "Both"};
        int[] axisValues = new int[] {0, 1, 2};
        axisValue = EditorGUILayout.IntPopup(axisValue, axis, axisValues);
        EditorGUILayout.EndHorizontal();
        if (target != null)
            targets = target.GetComponentsInChildren<Transform>();
        if (GUILayout.Button("Want Position"))
        {
            if (target != null)
            {
                if (selectChildren)
                {
                    foreach (Transform pos in targets)
                    {
                        if (pos != targets[0])
                        {
                            Calcurate(pos);
                        }
                    }
                }
                else
                    Calcurate(target.transform);
            }
        }
        if (GUILayout.Button("Set Name"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (i != 0)
                {
                    targets[i].name = (targets.Length - i).ToString();
                }
            }
        }
        scale = EditorGUILayout.Vector3Field("Scale", scale);
        if (GUILayout.Button("Scale Set X"))
        {
            //for (int i = 0; i < targets.Length; i++)
            //{
            //    if (i != 0)
            //    {
            //        targets[i].collider.isTrigger = true;
            //        targets[i].collider.= true;
            //    }
            //}
            MeshCollider[] a = target.GetComponentsInChildren<MeshCollider>();
            foreach (MeshCollider pos in a)
            {
                DestroyImmediate(pos);   
            }
        }
        if (GUILayout.Button("Scale Set Y"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (i != 0)
                {
                    targets[i].localScale = new Vector3(scale.x, targets[i].localScale.y, targets[i].localScale.z);
                }
            }
        }
        if (GUILayout.Button("Scale Set Z"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (i != 0)
                {
                    targets[i].localScale = new Vector3(targets[i].localScale.x, targets[i].localScale.y, scale.z);
                }
            }
        }
    }

    void Calcurate(Transform pos)
    {
        RaycastHit hit;
        //Terrain.activeTerrain.SampleHeight(pos.position) + gap
        Vector3 height = pos.position;

        if (Physics.Raycast(height, -Vector3.up, out hit))
        {
            pos.position = hit.point;
        }
        
        if (angleRotate)
        {
            Vector3 x = Vector3.zero;
            Vector3 z = Vector3.zero;
            Vector3 targetPosX = Vector3.zero;
            Vector3 targetPosZ = Vector3.zero;
            if (axisValue == 0 || axisValue == 2)
            {
                x = new Vector3(2, 0, 0);
                targetPosX = pos.position + x;
            }
            if (axisValue == 1 || axisValue == 2)
            {   
                z = new Vector3(0, 0, 2);
                targetPosZ = pos.position + z;
            }
            
            float heightGapX = Terrain.activeTerrain.SampleHeight(pos.position) - Terrain.activeTerrain.SampleHeight(targetPosX);
            float heightGapZ = Terrain.activeTerrain.SampleHeight(pos.position) - Terrain.activeTerrain.SampleHeight(targetPosZ);
            float angleX = Mathf.Atan(2 / Mathf.Abs(heightGapX)) * Mathf.Rad2Deg;
            float angleZ = Mathf.Atan(2 / Mathf.Abs(heightGapZ)) * Mathf.Rad2Deg;

            Debug.Log("angleX = " + angleX);
            Debug.Log("angleZ = " + angleZ);

            if (axisValue == 0 || axisValue == 2)
            {
                if (heightGapX < 0)
                    pos.Rotate(0, 0, 90 - angleX);
                else
                    pos.Rotate(0, 0, angleX - 90);
            }
            if (axisValue == 1 || axisValue == 2)
            {
                if (heightGapZ < 0)
                    pos.Rotate(angleZ - 90, 0, 0);
                else
                    pos.Rotate(90 - angleZ, 0, 0);
            }
        }
    }
}
