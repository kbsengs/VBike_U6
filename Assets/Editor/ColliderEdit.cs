using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class ColliderEdit : EditorWindow {
	
    [MenuItem("ZOIT/ColliderEdit")]
    static void Init()
    {
        ColliderEdit window = (ColliderEdit)EditorWindow.GetWindow(typeof(ColliderEdit));
    }

    enum COLLIDER_TYPE
    {
        Delete = 0, 
        Create_Box = 1,
        Create_Sphere = 2,
        Create_Capsule = 3,
        Create_Mesh = 4
    }
    COLLIDER_TYPE mType = COLLIDER_TYPE.Create_Box;

    GameObject target;    
    Transform[] transforms;

    bool isTrigger = false;
    PhysicsMaterial pMaterial = null;
    bool gbRoot = true;    

    void OnGUI()
    {
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject));
        mType = (COLLIDER_TYPE)EditorGUILayout.EnumPopup("Collider Type :", mType);
        if (mType != COLLIDER_TYPE.Delete)
        {
            isTrigger = EditorGUILayout.Toggle("Is Trigger?", isTrigger);
            pMaterial = (PhysicsMaterial)EditorGUILayout.ObjectField("PhysicMaterial", pMaterial, typeof(PhysicsMaterial));
            
            gbRoot = EditorGUILayout.Toggle("skip Root?", gbRoot); 
        }

        if (GUILayout.Button(mType.ToString()))
        {
            if (target != null)
            {
                switch (mType)
                {
                    case COLLIDER_TYPE.Delete:
                        Select_Collider_Editor(null);
                        break;
                    case COLLIDER_TYPE.Create_Box:
                        Select_Collider_Editor(typeof(BoxCollider));
                        break;
                    case COLLIDER_TYPE.Create_Sphere:
                        Select_Collider_Editor(typeof(SphereCollider));
                        break;
                    case COLLIDER_TYPE.Create_Capsule:
                        Select_Collider_Editor(typeof(CapsuleCollider));
                        break;
                    case COLLIDER_TYPE.Create_Mesh:
                        Select_Collider_Editor(typeof(MeshCollider));
                        break;
                }
            }
        }
    }
	void Select_Collider_Editor( System.Type tName )
	{
        transforms = target.GetComponentsInChildren<Transform>();
		
		if( tName == null )
		{
            for (int i = 0; i < transforms.Length; i++)
	        {
                EditorUtility.DisplayProgressBar(
                                            "���� ��!!",
                                            transforms[i].name,
                                            (i / (transforms.Length * 1.0f))
                                           );

                Collider[] obj = transforms[i].GetComponentsInChildren<Collider>();
				foreach( Collider p in obj)
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
				foreach( Component p in  obj)
				{
					p.gameObject.AddComponent( tName );
				}
			}
		}
        EditorUtility.ClearProgressBar();
	}
}