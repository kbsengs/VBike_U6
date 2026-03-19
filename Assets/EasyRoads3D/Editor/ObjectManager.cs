using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using EasyRoads3D;
using EasyRoads3DEditor;
public class ObjectManager : EditorWindow
{

public static ObjectManager instance;
public ObjectManager()
{

if( instance != null ){



}
instance = this;
title = "EasyRoads3D Object Manager";
position = new Rect((Screen.width - 550.0f) / 2.0f, (Screen.height - 400.0f) / 2.0f, 550.0f, 400.0f);
minSize = new Vector2(550.0f, 400.0f);
maxSize = new Vector2(550.0f, 400.0f);
OQQCCDDQOO.OQOQDDCDQO();

}
public void OnDestroy(){
if(ProceduralObjectsEditor.instance != null){
ProceduralObjectsEditor.instance.Close();
}
OQQCCDDQOO.newObjectFlag = false;
OQQCCDDQOO.duplicateObjectFlag = false;
instance = null;
}
public static ObjectManager Instance{
get
{
if( instance == null ){
new ObjectManager();
}
return instance;
}
}
public void OnGUI()
{
ArrayList r = OQQCCDDQOO.OQCQCCQDQC();

if(r.Count == 5){





ProceduralObjectsEditor editor = null;
if(ProceduralObjectsEditor.instance == null){

editor = (ProceduralObjectsEditor)ScriptableObject.CreateInstance(typeof(ProceduralObjectsEditor));
}else{
editor = ProceduralObjectsEditor.instance;

}
editor.position = new Rect (editor.position.x, editor.position.y, 500, 400);

editor.title = (string)r[0];



ODODDQQO so = (ODODDQQO)r[4];

editor.DisplayNodes((int)r[1], so, (GameObject)r[3]);
editor.Show();

}else if(r.Count == 1){

ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
RoadObjectScript.ODODOQQO = ODODDCCOQO.OOQOOQODQQ(arr);
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
foreach(RoadObjectScript scr in scripts){

scr.OOOOOOODCD(arr, ODODDCCOQO.OOQOOQODQQ(arr), ODODDCCOQO.OQQDOODOOQ(arr));
}
if(ProceduralObjectsEditor.instance != null){
ProceduralObjectsEditor.instance.Close();
}
instance.Close();
}else if(r.Count == 3){
SideObjectImporter ieditor = (SideObjectImporter)ScriptableObject.CreateInstance(typeof(SideObjectImporter));
SideObjectImporter.sideobjects =  (String[]) r[0];
SideObjectImporter.flags = new bool[(int)r[1]];

SideObjectImporter.importedSos = (ArrayList)r[2];
ieditor.ShowUtility();

}else if(r.Count == 2){

ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
RoadObjectScript.ODODOQQO = ODODDCCOQO.OOQOOQODQQ(arr);
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
foreach(RoadObjectScript scr in scripts){

ArrayList arr1  = ODODDCCOQO.OCDCQOOODO(false);
if(scr.OOQQCODOCD == null) {

scr.OCOQDDODDQ(arr1, ODODDCCOQO.OOQOOQODQQ(arr1), ODODDCCOQO.OQQDOODOOQ(arr1));
}
scr.OOOOOOODCD(arr1, ODODDCCOQO.OOQOOQODQQ(arr1), ODODDCCOQO.OQQDOODOOQ(arr1));
if(scr.ODODCOCCDQ == true || scr.objectType == 2){
GameObject go = GameObject.Find(scr.gameObject.name+"/Side Objects/"+r[0]);

if(go != null){

OCCQOOQDDO.OCOOCOOQOD((sideObjectScript)go.GetComponent(typeof(sideObjectScript)), (int)r[1], scr, go.transform);
}
}
}
}
}

}
