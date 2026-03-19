using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using EasyRoads3D;
using EasyRoads3DEditor;
public class EasyRoadsEditorMenu : ScriptableObject {







[MenuItem( "EasyRoads3D/New Object" )]
public static void  CreateEasyRoads3DObject ()
{

Terrain[] terrains = (Terrain[]) FindObjectsOfType(typeof(Terrain));
if(terrains.Length == 0){
EditorUtility.DisplayDialog("Alert", "No Terrain objects found! EasyRoads3D objects requires a terrain object to interact with. Please create a Terrain object first", "Close");
return;
}



if(NewEasyRoads3D.instance == null){
NewEasyRoads3D window = (NewEasyRoads3D)ScriptableObject.CreateInstance(typeof(NewEasyRoads3D));
window.ShowUtility();
}



}
[MenuItem( "EasyRoads3D/Back Up/Terrain Height Data" )]
public static void  GetTerrain ()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.OOCDQCCOOC(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Restore/Terrain Height Data" )]
public static void  SetTerrain ()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.ODQCDCCDDQ(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Back Up/Terrain Splatmap Data" )]
public static void  OOOCQOQQQC()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.OOOCQOQQQC(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Restore/Terrain Splatmap Data" )]
public static void  ODDDCDDCDQ ()
{
if(GetEasyRoads3DObjects()){
string path = "";
if(EditorUtility.DisplayDialog("Road Splatmap", "Would you like to merge the terrain splatmap(s) with a road splatmap?", "Yes", "No")){
path = EditorUtility.OpenFilePanel("Select png road splatmap texture", "", "png");
}


ODCDDDDQQD.ODCODOCDDD(true, 100, 4, path, Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Back Up/Terrain Vegetation Data" )]
public static void  OQOCQQOCQQ()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.OQOCQQOCQQ(Selection.activeGameObject, null, "");
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Back Up/All Terrain Data" )]
public static void  GetAllData()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.OOCDQCCOOC(Selection.activeGameObject);
ODCDDDDQQD.OOOCQOQQQC(Selection.activeGameObject);
ODCDDDDQQD.OQOCQQOCQQ(Selection.activeGameObject, null,"");
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Restore/Terrain Vegetation Data" )]
public static void  OOQCCOODOD()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.OOQCCOODOD(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "EasyRoads3D/Restore/All Terrain Data" )]
public static void  RestoreAllData()
{
if(GetEasyRoads3DObjects()){

ODCDDDDQQD.ODQCDCCDDQ(Selection.activeGameObject);
ODCDDDDQQD.ODCODOCDDD(true, 100, 4, "", Selection.activeGameObject);
ODCDDDDQQD.OOQCCOODOD(Selection.activeGameObject);

}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}


}

[MenuItem ("EasyRoads3D/Side Objects/Object Manager")]
static void ShowObjectManager ()
{

if(RoadObjectScript.erInit == ""){
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
if(scripts != null) Selection.activeGameObject = scripts[0].gameObject;


}
if(ObjectManager.instance == null){

if(Terrain.activeTerrain != null)OQQCCDDQOO.terrainTrees = OQCDQQDQCC.OQDQCQODOC();
ObjectManager window =(ObjectManager)ScriptableObject.CreateInstance(typeof(ObjectManager));
window.ShowUtility();
}
}


[MenuItem( "EasyRoads3D/Build EasyRoads3D Objects" )]
public static void  FinalizeRoads ()
{

bool destroyTerrainScript = true;
if(EditorUtility.DisplayDialog("Build EasyRoads3D Objects", "This process includes destroying all EasyRoads3D control objects. Did you make a backup of the Scene? Do you want to continue?\n\nDepending on the number of EasyRoads3D objects in the Scene and used side objects, this process may take a while. Please be patient. ", "Yes", "No")){
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
foreach (RoadObjectScript script in scripts) {
bool renderflag = true;
bool renderAlreadyDone = false;
int num = 0;
if(!script.displayRoad){
num = EditorUtility.DisplayDialogComplex ("Disabled EasyRoads3D Object Detected:", script.gameObject.name + " is currently not displayed.\n\nWould you like to activate and finalize this object, destroy this object or skip it in this finalize procedure?", "Finalize", "Destroy", "Skip");
if(num == 0){
script.displayRoad = true;
if(script.OOQQCODOCD == null){
ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
script.OCOQDDODDQ(arr, ODODDCCOQO.OOQOOQODQQ(arr), ODODDCCOQO.OQQDOODOOQ(arr));
}
script.OOQQCODOCD.OODDDCQCCQ(script.displayRoad, script.OODDDCQCOC);
}
if(num == 1){

renderflag = false;
}
if(num == 2){
renderflag = false;
destroyTerrainScript = false;
}
}
if(script.transform != null && renderflag && !script.ODODCOCCDQ){
if(script.OOQQCODOCD == null){
ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
script.OCOQDDODDQ(arr, ODODDCCOQO.OOQOOQODQQ(arr), ODODDCCOQO.OQQDOODOOQ(arr));
}

if(RoadObjectScript.erInit == ""){
RoadObjectScript.erInit = ODCQDOODQC.OQCQCDQDCC(RoadObjectScript.version); 
OQCDQQDQCC.erInit = RoadObjectScript.erInit;
}

if(script.OOQQCODOCD == null){
script.ODOCOQCCOC(script.transform, null, null, null);
}
OQCDQQDQCC.ODOQCCODQC = true;
if(!script.ODODCOCCDQ){
script.geoResolution = 0.5f;
script.OOQODQOCOC();
if(script.objectType < 2) OOOODOCQQQ(script);
script.OOQQCODOCD.terrainRendered = true;
script.OCQDCQDDCO();



}
if(script.displayRoad && script.objectType < 2){

if(script.objectType == 1){

SetWaterScript(script);
}
script.OOQQCODOCD.road.transform.parent = null;
script.OOQQCODOCD.road.layer = 0;
script.OOQQCODOCD.road.name = script.gameObject.name;
}
else if(script.OOQQCODOCD.road != null)DestroyImmediate(script.OOQQCODOCD.road);



bool flag = false;
for(int i=0;i<script.ODODQQOD.Length;i++){
if(script.ODODQQOD[i]){
flag = true;
break;
}
}
if(flag){
OCCQOOQDDO.OCCQQQQDQC(script);
}
foreach(Transform child in script.transform){
if(child.name == "Side Objects"){
child.name = script.gameObject.name + " - Side Objects ";
child.parent = null;
}
}
}else if(script.ODODCOCCDQ){
renderAlreadyDone = true;
destroyTerrainScript = false;
}
if((script.displayRoad || num != 2) && !renderAlreadyDone)DestroyImmediate(script.gameObject);
}

if(destroyTerrainScript){
EasyRoads3DTerrainID[] terrainscripts = (EasyRoads3DTerrainID[])FindObjectsOfType(typeof(EasyRoads3DTerrainID));
foreach (EasyRoads3DTerrainID script in terrainscripts) {
DestroyImmediate(script);
}
}
}
}

public static bool GetEasyRoads3DObjects(){
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
bool flag = false;
foreach (RoadObjectScript script in scripts) {
if(script.OOQQCODOCD == null){

ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
script.ODOCOQCCOC(script.transform, arr, ODODDCCOQO.OOQOOQODQQ(arr), ODODDCCOQO.OQQDOODOOQ(arr));


}
flag = true;
}
return flag;
}
static private void OOOODOCQQQ(RoadObjectScript target){
EditorUtility.DisplayProgressBar("Build EasyRoads3D Object - " + target.gameObject.name,"Initializing", 0);

RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
ArrayList rObj = new ArrayList();
Undo.RegisterUndo(Terrain.activeTerrain.terrainData, "EasyRoads3D Terrain leveling");
foreach(RoadObjectScript script in scripts) {
if(script.transform != target.transform) rObj.Add(script.transform);
}
if(target.ODODQOQO == null){
target.ODODQOQO = target.OOQQCODOCD.OCDODCOCOC();
target.ODODQOQOInt = target.OOQQCODOCD.OCCQOQCQDO();
}
target.OCOOCODDOC(0.5f, true, false);

ArrayList hitOQQOCQDCOQ = target.OOQQCODOCD.OCOOOOCOQO(Vector3.zero, target.raise, target.obj, target.OOQDOOQQ, rObj, target.handleVegetation);
ArrayList changeArr = new ArrayList();
float stepsf = Mathf.Floor(hitOQQOCQDCOQ.Count / 10);
int steps = Mathf.RoundToInt(stepsf);
for(int i = 0; i < 10;i++){
changeArr = target.OOQQCODOCD.OQODDQOQDQ(hitOQQOCQDCOQ, i * steps, steps, changeArr);
EditorUtility.DisplayProgressBar("Build EasyRoads3D Object - " + target.gameObject.name,"Updating Terrain", i * 10);
}

changeArr = target.OOQQCODOCD.OQODDQOQDQ(hitOQQOCQDCOQ, 10 * steps, hitOQQOCQDCOQ.Count - (10 * steps), changeArr);
target.OOQQCODOCD.OODDQOQDDD(changeArr, rObj);

target.OCQDCQDDCO();
EditorUtility.ClearProgressBar();

}
private static void SetWaterScript(RoadObjectScript target){
for(int i = 0; i < target.OOOOCOCCDC.Length; i++){
if(target.OOQQCODOCD.road.GetComponent(target.OOOOCOCCDC[i]) != null && i != target.selectedWaterScript)DestroyImmediate(target.OOQQCODOCD.road.GetComponent(target.OOOOCOCCDC[i]));
}
if(target.OOOOCOCCDC[0] != "None Available!"  && target.OOQQCODOCD.road.GetComponent(target.OOOOCOCCDC[target.selectedWaterScript]) == null){
UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(target.OOQQCODOCD.road, "Assets/EasyRoads3D/Editor/EasyRoadsEditorMenu.cs (301,1)", target.OOOOCOCCDC[target.selectedWaterScript]);

}
}
public static Vector3 ReadFile(string file)
{
Vector3 pos = Vector3.zero;
if(File.Exists(file)){
StreamReader streamReader = File.OpenText(file);
string line = streamReader.ReadLine();
line = line.Replace(",",".");
string[] lines = line.Split("\n"[0]);
string[] arr = lines[0].Split("|"[0]);
float.TryParse(arr[0],System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out pos.x);
float.TryParse(arr[1],System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out pos.y);
float.TryParse(arr[2],System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out pos.z);
}
return pos;
}
}
