using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(WaypointDefine))]
public class WaypointDefineEditor : Editor {

    WaypointDefine _WaypointDefine;

    Transform way_endpoint;
    Transform way_startpoint;
    int cross;
    Transform[] crossStart;
    int[] crossStartNum;
    Transform[] start;
    Transform[] end;

    bool crossSetting;
    bool crossList;
    bool crossStartPoints;

    bool showAllpoint = false;
    bool showStartTarget = false;
    bool showWay = false;
    bool showCross = false;
    bool cross_1 = false;
    bool cross_2 = false;
    bool cross_3 = false;
    bool cross_4 = false;
    bool cross_5 = false;

    Vector2 scroll1;
    Vector2 scroll2;

    void OnEnable()
    {
        _WaypointDefine = target as WaypointDefine;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PrefixLabel("Tool");
        way_endpoint = (Transform)EditorGUILayout.ObjectField("Way End", way_endpoint, typeof(Transform));
        way_startpoint = (Transform)EditorGUILayout.ObjectField("Way Start Target", way_startpoint, typeof(Transform));
        EditorGUILayout.BeginHorizontal();
        cross = EditorGUILayout.IntField("Corss", cross);
        if (GUILayout.Button("Set"))
        {
            start = new Transform[cross];
            end = new Transform[cross];
            crossStart = new Transform[cross];
            if (cross != 0) crossSetting = true;
            else crossSetting = false;
        }
        EditorGUILayout.EndHorizontal();
        if (crossSetting)
        {
            crossStartPoints = EditorGUILayout.Foldout(crossStartPoints, "CrossRoad Start");
            if (crossStartPoints)
            {
                for (int i = 0; i < cross; i++)
                {
                    crossStart[i] = (Transform)EditorGUILayout.ObjectField("CrossStart_" + (i + 1).ToString(), crossStart[i], typeof(Transform));
                }
            }
            crossList = EditorGUILayout.Foldout(crossList, "CrossList");
            if (crossList)
            {
                for (int i = 0; i < cross; i++ )
                {
                    start[i] = (Transform)EditorGUILayout.ObjectField((i + 1).ToString() + "_Start", start[i], typeof(Transform));
                    end[i] = (Transform)EditorGUILayout.ObjectField((i + 1).ToString() + "_End", end[i], typeof(Transform));
                    EditorGUILayout.Space();
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("WayDefine"))
        {
            Transform[] objs = _WaypointDefine.transform.GetComponentsInChildren<Transform>();
            if (objs.Length > 1 && way_endpoint != null)
            {
                _WaypointDefine.allways = new Transform[objs.Length - 1];
                
                for (int i = 0; i < _WaypointDefine.allways.Length; i++)
                {
                    _WaypointDefine.allways[i] = objs[i + 1];
                    if (_WaypointDefine.allways[i] == way_endpoint)
                    {
                        _WaypointDefine.ways[1] = i;
                    }
                    if (_WaypointDefine.allways[i] == way_startpoint)
                    {
                        _WaypointDefine.startpoint = i;
                    }
                }

                if (start != null)
                {
                    crossStartNum = new int[crossStart.Length];
                    for (int i = 0; i < _WaypointDefine.allways.Length; i++)
                    {
                        for (int j = 0; j < crossStart.Length; j++)
                        {
                            if (crossStart[j] == _WaypointDefine.allways[i])
                            {
                                crossStartNum[j] = i;
                            }
                        }
                    }
                    _WaypointDefine.crossStart = crossStartNum;
                    for (int i = 0; i < start.Length; i++)
                    {
                        int s = 0;
                        int e = 0;
                        for (int j = 0; j < _WaypointDefine.allways.Length; j++)
                        {
                            if (_WaypointDefine.allways[j] == start[i])
                            {
                                s = j;
                            }
                            else if (_WaypointDefine.allways[j] == end[i])
                            {
                                e = j;
                            }
                        }
                        switch (i)
                        {
                            case 0:
                                _WaypointDefine.crossways_1[0] = s;
                                _WaypointDefine.crossways_1[1] = e;
                                break;
                            case 1:
                                _WaypointDefine.crossways_2[0] = s;
                                _WaypointDefine.crossways_2[1] = e;
                                break;
                            case 2:
                                _WaypointDefine.crossways_3[0] = s;
                                _WaypointDefine.crossways_3[1] = e;
                                break;
                            case 3:
                                _WaypointDefine.crossways_4[0] = s;
                                _WaypointDefine.crossways_4[1] = e;
                                break;
                            case 4:
                                _WaypointDefine.crossways_5[0] = s;
                                _WaypointDefine.crossways_5[1] = e;
                                break;
                        }
                    }
                }
                Debug.Log("Way Define Comlete");
            }
            else if (way_endpoint == null)
            {
                Debug.Log("Don't set Way End");
            }
            else
            {
                Debug.Log("Don't have Waypoint");
            }
        }
        if (GUILayout.Button("Clear"))
        {
            _WaypointDefine.allways = null;
            _WaypointDefine.crossStart = null;
            _WaypointDefine.ways = new int[2];
            _WaypointDefine.crossways_1 = new int[2];
            _WaypointDefine.crossways_2 = new int[2];
            _WaypointDefine.crossways_3 = new int[2];
            _WaypointDefine.crossways_4 = new int[2];
            _WaypointDefine.crossways_5 = new int[2];
            _WaypointDefine.startpoint = 0;
            cross = 0;
            crossSetting = false;
            way_endpoint = null;
            start = null;
            end = null;
            crossStart = null;
            
            Debug.Log("All Value Clear");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        showAllpoint = EditorGUILayout.Foldout(showAllpoint, "Allpoints");
        if (showAllpoint)
        {
            scroll1 = EditorGUILayout.BeginScrollView(scroll1);
            if (_WaypointDefine.allways != null)
            {
                foreach (Transform pos in _WaypointDefine.allways)
                {
                    Transform o = (Transform)EditorGUILayout.ObjectField(pos.name, pos, typeof(Transform));
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.Space();

        showWay = EditorGUILayout.Foldout(showWay, "Waypoints");
        if (showWay)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.ways[0] = EditorGUILayout.IntField("start", _WaypointDefine.ways[0]);
            _WaypointDefine.ways[1] = EditorGUILayout.IntField("end", _WaypointDefine.ways[1]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        showStartTarget = EditorGUILayout.Foldout(showStartTarget, "StartTarget");
        if (showStartTarget)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.startpoint = EditorGUILayout.IntField("target", _WaypointDefine.startpoint);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        showCross = EditorGUILayout.Foldout(showCross, "Cross Start");
        if (showCross)
        {
            if (_WaypointDefine.crossStart != null)
            {
                for (int i = 0; i < _WaypointDefine.crossStart.Length; i++)
                {
                    _WaypointDefine.crossStart[i] = EditorGUILayout.IntField("cross_" + (i + 1).ToString(), _WaypointDefine.crossStart[i]);
                }
            }
        }
        EditorGUILayout.Space();

        cross_1 = EditorGUILayout.Foldout(cross_1, "Cross_1");
        if (cross_1)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.crossways_1[0] = EditorGUILayout.IntField("start", _WaypointDefine.crossways_1[0]);
            _WaypointDefine.crossways_1[1] = EditorGUILayout.IntField("end", _WaypointDefine.crossways_1[1]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        cross_2 = EditorGUILayout.Foldout(cross_2, "Cross_2");
        if (cross_2)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.crossways_2[0] = EditorGUILayout.IntField("start", _WaypointDefine.crossways_2[0]);
            _WaypointDefine.crossways_2[1] = EditorGUILayout.IntField("end", _WaypointDefine.crossways_2[1]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        cross_3 = EditorGUILayout.Foldout(cross_3, "Cross_3");
        if (cross_3)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.crossways_3[0] = EditorGUILayout.IntField("start", _WaypointDefine.crossways_3[0]);
            _WaypointDefine.crossways_3[1] = EditorGUILayout.IntField("end", _WaypointDefine.crossways_3[1]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        cross_4 = EditorGUILayout.Foldout(cross_4, "Cross_4");
        if (cross_4)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.crossways_4[0] = EditorGUILayout.IntField("start", _WaypointDefine.crossways_4[0]);
            _WaypointDefine.crossways_4[1] = EditorGUILayout.IntField("end", _WaypointDefine.crossways_4[1]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        cross_5 = EditorGUILayout.Foldout(cross_5, "Cross_5");
        if (cross_5)
        {
            EditorGUILayout.BeginHorizontal();
            _WaypointDefine.crossways_5[0] = EditorGUILayout.IntField("start", _WaypointDefine.crossways_5[0]);
            _WaypointDefine.crossways_5[1] = EditorGUILayout.IntField("end", _WaypointDefine.crossways_5[1]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
    }
}
