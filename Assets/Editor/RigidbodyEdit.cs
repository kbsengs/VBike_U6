using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class RigidbodyEdit : EditorWindow
{
    [MenuItem("ZOIT/RigidbodyEdit")]
    static void Init()
    {
        RigidbodyEdit window = (RigidbodyEdit)EditorWindow.GetWindow(typeof(RigidbodyEdit));
    }
    GameObject target;
    Rigidbody[] targets;
    Transform[] transforms;
    public float gMass = 1;
    public float gDrag = 0;
    public float gAngularDrag = 0.05f;
    public bool gGravity = true;
    public bool gKinematic = false;
    public RigidbodyInterpolation gInterplate = RigidbodyInterpolation.None;
    public CollisionDetectionMode gCollisionDetection = CollisionDetectionMode.ContinuousDynamic;
    public bool gbRoot = true;
    

    void OnGUI()
    {
        //selectChildren = EditorGUILayout.Toggle("SelectChildren", selectChildren);
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject));
        gMass = EditorGUILayout.FloatField("Mass :", gMass);
        gDrag = EditorGUILayout.FloatField("Drag :", gDrag);
        gAngularDrag = EditorGUILayout.FloatField("AngularDrag :", gAngularDrag);
        gGravity = EditorGUILayout.Toggle("Use Gravity :", gGravity);
        gKinematic = EditorGUILayout.Toggle("Use Kinematic :", gKinematic);
        gInterplate = (RigidbodyInterpolation)EditorGUILayout.EnumPopup("Intrepolate :", gInterplate);
        gCollisionDetection = (CollisionDetectionMode)EditorGUILayout.EnumPopup("CollisionDetection :", gCollisionDetection);
        gbRoot = EditorGUILayout.Toggle("skip Root? :", gbRoot);        

        if (GUILayout.Button("Create"))
        {
            if (target != null)
            {
                Select_Rigidbody(true);
                Config_Rigidbody();
                //this.Close();
            }
            else
                EditorUtility.DisplayDialog("Not Find Source Object!", "������Ʈ�� �巡���ؼ� Target�� �־��ֻ�.", "OK");
        }
        else if (GUILayout.Button("Config"))
        {
            if (target != null)
            {
                Config_Rigidbody();
                //this.Close();
            }
            else
                EditorUtility.DisplayDialog("Not Find Source Object!", "������Ʈ�� �巡���ؼ� Target�� �־��ֻ�.", "OK");
        }
        else if (GUILayout.Button("Delete"))
        {
            if (target != null)
            {
                Select_Rigidbody(false);
                //this.Close();
            }
            else
                EditorUtility.DisplayDialog("Not Find Source Object!", "������Ʈ�� �巡���ؼ� Target�� �־��ֻ�.", "OK");            
        }
        
    }

    void Config_Rigidbody()
    {
        targets = target.GetComponentsInChildren<Rigidbody>();
        //foreach (MeshRenderer pos in targets)
        for (int k = 0; k < targets.Length; k++)
        {
            EditorUtility.DisplayProgressBar(
                                            "���� ��!!",
                                            targets[k].name,
                                            (k / (targets.Length * 1.0f))
                                           );
            targets[k].GetComponent<Rigidbody>().mass = gMass;
            targets[k].GetComponent<Rigidbody>().linearDamping = gDrag;
            targets[k].GetComponent<Rigidbody>().angularDamping = gAngularDrag;
            targets[k].GetComponent<Rigidbody>().interpolation = gInterplate;
            targets[k].GetComponent<Rigidbody>().useGravity = gGravity;
            targets[k].isKinematic = gKinematic; // Unity6 Migration: .Rb was Cycle_Control cache, use Rigidbody directly here
            targets[k].GetComponent<Rigidbody>().collisionDetectionMode = gCollisionDetection;
            
        }
        EditorUtility.ClearProgressBar();
    }
    void Select_Rigidbody(bool b)
    {
        transforms = target.GetComponentsInChildren<Transform>();
        if (!b)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                EditorUtility.DisplayProgressBar(
                                            "���� ��!!",
                                            transforms[i].name,
                                            (i / (transforms.Length * 1.0f))
                                           );
                Rigidbody[] obj = transforms[i].GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody p in obj)
                {
                    Debug.Log(p);
                    DestroyImmediate(p);
                }                
            }
        }
        else
        {
            for (int i = Convert.ToInt32(gbRoot); i < transforms.Length; i++)
            {
                EditorUtility.DisplayProgressBar(
                                            "�߰� ��!!",
                                            transforms[i].name,
                                            (i / (transforms.Length * 1.0f))
                                           );
                Component[] obj = transforms[i].GetComponentsInChildren(typeof(Component));                
                foreach (Component p in obj)
                {
                    p.gameObject.AddComponent<Rigidbody>();
                }
                
            }
        }
        EditorUtility.ClearProgressBar();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}