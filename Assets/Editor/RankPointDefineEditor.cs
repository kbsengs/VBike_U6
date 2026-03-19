using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RankPointDefine))]
public class RankPointDefineEditor : Editor {

    RankPointDefine _RankPointDefine;

    int num;
    Transform[] rankpoints;
    //Transform[] allpoints;
    int[] rankpoints_int;
    bool show1;
    bool show2;
    bool foldRankPoints;

    void OnEnable()
    {
        _RankPointDefine = target as RankPointDefine; 
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        num = EditorGUILayout.IntField("RankPoint Count", num);
        if (GUILayout.Button("Set"))
        {
            rankpoints = new Transform[num];
            if (num > 0) show1 = true;
            else show1 = false;
        }
        EditorGUILayout.EndHorizontal();
        if (show1)
        {
            foldRankPoints = EditorGUILayout.Foldout(foldRankPoints, "RankPoins Set");
            if (foldRankPoints)
            {
                for (int i = 0; i < num; i++)
                {
                    rankpoints[i] = (Transform)EditorGUILayout.ObjectField("RankPoint_" + (i + 1).ToString(), rankpoints[i], typeof(Transform));
                }
            }
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Define"))
        {
            if (rankpoints.Length > 0)
            {
                Transform [] allpoints = _RankPointDefine.transform.GetComponentsInChildren<Transform>();
                rankpoints_int = new int[rankpoints.Length];
                for (int i = 0; i < allpoints.Length; i++)
                {
                    for (int j = 0; j < rankpoints.Length; j++)
                    {
                        if (allpoints[i] == rankpoints[j])
                        {
                            rankpoints_int[j] = i - 1;
                        }
                    }
                }
                _RankPointDefine.rankpoint = rankpoints_int;
                Debug.Log("Rank Define");
            }
        }
        if (GUILayout.Button("Clear"))
        {
            _RankPointDefine.rankpoint = null;
            rankpoints = null;
            num = 0;
            show1 = false;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        show2 = EditorGUILayout.Foldout(show2, "RankPoints");
        if (show2)
        {
            if (_RankPointDefine.rankpoint != null)
            {
                for (int i = 0; i < _RankPointDefine.rankpoint.Length; i++)
                {
                    _RankPointDefine.rankpoint[i] = EditorGUILayout.IntField("point_" + (i + 1).ToString(), _RankPointDefine.rankpoint[i]);
                }
            }
        }
    }
}
