using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using EasyRoads3D;
using EasyRoads3DEditor;
public class ProceduralObjectsEditor : EditorWindow
{

public static ProceduralObjectsEditor instance;
public OQODQQOOCD so_editor;
public int sideObject;
private ODODDQQO so;

private GameObject so_go;

string[] traceStrings;

public ProceduralObjectsEditor() {
instance = this;
}
void OnDestroy(){
OOQCQODQOC.OnDestroy1();
instance = null;
}
public void DisplayNodes (int index, ODODDQQO soi, GameObject sso_go)

{
so_go = sso_go;
ArrayList tmpNodes = new ArrayList();
if(soi != null) tmpNodes.AddRange(soi.nodeList);

if(so_go != null && tmpNodes.Count == 0){

ArrayList arr = OQODQQOOCD.OCQDQCQQOC(2, so_go, OOQCQODQOC.traceOffset);
if(arr != null){
if(arr.Count > 1){
tmpNodes = arr;
}
}
}
bool clamped = false;
so = soi;
sideObject = index;
if (so_editor == null){
try{
so_editor = new OQODQQOOCD(position, tmpNodes, clamped);
}catch{
}
}



if(so_editor.OQQOCQDCOQ.Count > 0){
if((Vector2)so_editor.OQQOCQDCOQ[0] == (Vector2)so_editor.OQQOCQDCOQ[so_editor.OQQOCQDCOQ.Count - 1]){

so_editor.closed = true;
so_editor.OQQOCQDCOQ.RemoveAt(so_editor.OQQOCQDCOQ.Count - 1);
}
}
if(tmpNodes.Count != 0){
Rect rect = new Rect(stageSelectionGridWidth, 0, Screen.width - stageSelectionGridWidth, Screen.height);
so_editor.FrameSelected(rect);
}
OOQCQODQOC.OCCOQCCQCO(index, soi, sso_go, so_editor);
return;
}
void OnGUI ()
{
Rect rect = new Rect(stageSelectionGridWidth, 0, Screen.width - stageSelectionGridWidth, Screen.height);
EditorGUILayout.BeginHorizontal();
GUILayout.Space(210);
GUILayout.Label(new GUIContent("Hit [r] to center the editor, hit [z] to zoom in on the nodes, click drag to move the canvas, Scrollwheel (or [shift] click drag) zoom in / out", ""), GUILayout.Width(800) );
EditorGUILayout.EndHorizontal();
GUILayout.Space(-15);
OOQCQODQOC.OQCQCCQDQC(rect);
DoGUI ();
so_editor.OnGUI(rect);
}
float stageSelectionGridWidth = 200;
void DoGUI ()
{

EditorGUILayout.BeginHorizontal();
GUILayout.Space(60);
if(GUILayout.Button ("Apply", GUILayout.Width(65))){
OOQCQODQOC.OOQDQDQQOQ();
instance.Close();
}
if(GUILayout.Button ("Close", GUILayout.Width(65))){
instance.Close();
}
EditorGUILayout.EndHorizontal();
GUILayout.Space(5);
if(so_editor.isChanged == false) GUI.enabled = false;
EditorGUILayout.BeginHorizontal();
GUILayout.Space(60);
if(GUILayout.Button ("Update Scene", GUILayout.Width(135))){

so.nodeList.Clear();
if(so_editor.closed) so_editor.OQQOCQDCOQ.Add(so_editor.OQQOCQDCOQ[0]);
so.nodeList.AddRange(so_editor.OQQOCQDCOQ);
so_editor.isChanged = false;
OQQCCDDQOO.ODDCCOCDQC(OQQCCDDQOO.selectedObject);
OQQCCDDQOO.OODDCCDDQO();

ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
RoadObjectScript.ODODOQQO = ODODDCCOQO.OOQOOQODQQ(arr);
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
foreach(RoadObjectScript scr in scripts){

if(scr.OOQQCODOCD == null) {
ArrayList arr1  = ODODDCCOQO.OCDCQOOODO(false);
scr.OCOQDDODDQ(arr1, ODODDCCOQO.OOQOOQODQQ(arr1), ODODDCCOQO.OQQDOODOOQ(arr1));
}
scr.OOOOOOODCD(arr, ODODDCCOQO.OOQOOQODQQ(arr), ODODDCCOQO.OQQDOODOOQ(arr));
if(scr.ODODCOCCDQ == true || scr.objectType == 2){
GameObject go = GameObject.Find(scr.gameObject.name+"/Side Objects/"+so.name);


if(go != null){
OCCQOOQDDO.OCOOCOOQOD((sideObjectScript)go.GetComponent(typeof(sideObjectScript)), sideObject, scr, go.transform);
}
}
}
}
EditorGUILayout.EndHorizontal();
GUI.enabled = true;
if (GUI.changed)
{
so_editor.isChanged = true;

}
Handles.color = Color.black;
Handles.DrawLine(new Vector2 (stageSelectionGridWidth,0), new Vector2 (stageSelectionGridWidth,Screen.height));

Handles.DrawLine(new Vector2 (stageSelectionGridWidth - 1,0), new Vector2 (stageSelectionGridWidth - 1,Screen.height));

}

}
