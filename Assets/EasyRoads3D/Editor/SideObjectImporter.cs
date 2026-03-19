using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using EasyRoads3D;
using EasyRoads3DEditor;
public class SideObjectImporter : EditorWindow
{

public static SideObjectImporter instance;
public static Vector2 scrollPosition;
public static String[] sideobjects;
public static bool[] flags;
public static string[] soStrings;
public static ArrayList importedSos = new ArrayList();
public SideObjectImporter()
{
if( instance != null ){
}
instance = this;
title = "Import Side Objects";
position = new Rect((Screen.width - 250.0f) / 2.0f, (Screen.height - 400.0f) / 2.0f, 250.0f, 400.0f);
minSize = new Vector2(250.0f, 200.0f);
maxSize = new Vector2(850.0f, 800.0f);

}
public void OnDestroy(){
instance = null;
}
public static SideObjectImporter Instance{
get
{
if( instance == null ){
new SideObjectImporter();
}
return instance;
}
}
public void OnGUI()
{

bool sel = false;
foreach(bool flag in flags){
if(flag){
sel = true;
break;
}
}
scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
int i = 0;
foreach(String so in sideobjects){
flags[i] = EditorGUILayout.Toggle (flags[i] , GUILayout.Width(15));
GUILayout.Space(-20);
GUILayout.Label("    "+so,GUILayout.Width(150));
i++;
}
EditorGUILayout.EndScrollView();
if(!sel)GUI.enabled = false;
EditorGUILayout.BeginHorizontal();
GUILayout.FlexibleSpace();
if(GUILayout.Button ("Import", GUILayout.Width(125), GUILayout.Height(25))){
ArrayList comboValues = new ArrayList();
for(i = 0; i < OQQCCDDQOO.roadObjects.Length; i++){
comboValues.Add(OQQCCDDQOO.roadObjects[i]);
}



for(i = 0; i < flags.Length; i++){
if(flags[i]){
ODODDQQO thisso = (ODODDQQO)importedSos[i];
if(CheckExists(thisso)){
}else{
thisso.name = CheckName(thisso.name);
comboValues.Add(thisso.name);
OQQCCDDQOO.ODCDCDQCQC.Add(thisso);
}
}
}

OQQCCDDQOO.roadObjects = (string[])comboValues.ToArray(typeof(string));
OQQCCDDQOO.OQDDQCOCOO();
ObjectManager.instance.Repaint();
instance.Close();
}
EditorGUILayout.EndHorizontal();
}
public static String CheckName(String sideobjectname){
for(int i = 0 ; i < OQQCCDDQOO.ODCDCDQCQC.Count;i++){
ODODDQQO so = (ODODDQQO)OQQCCDDQOO.ODCDCDQCQC[i];
if(so.name == sideobjectname){
sideobjectname = sideobjectname + "1";
sideobjectname = CheckName(sideobjectname);
}
}
return sideobjectname;
}
public static bool CheckExists(ODODDQQO so){
return false;
}

}
