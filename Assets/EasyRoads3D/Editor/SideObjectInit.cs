using UnityEngine;
using UnityEditor;
using System.Collections;
using EasyRoads3D;
public class OCCQOOQDDO{

static public void OCCQQQQDQC(RoadObjectScript target){


ODODDCCOQO.OQCDODCCQC(target.transform);

ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
target.OOOOOOODCD(arr, ODODDCCOQO.OOQOOQODQQ(arr), ODODDCCOQO.OQQDOODOOQ(arr));
Transform mySideObject = OQQQQDQCCO(target);
ODOQDOQCDC(target.OOQQCODOCD, target.transform, target.ODCOQCODCC(), target.OOQDOOQQ, target.OOQQQOQO, target.raise, target, mySideObject);



target.ODODDDOO = true;

}
static public void ODOQDOQCDC(OQCDQQDQCC OOQQCODOCD, Transform obj , ArrayList param , bool OOQDOOQQ ,  int[] activeODODDQQO , float raise, RoadObjectScript target , Transform mySideObject){
ArrayList pnts  = target.OOQQCODOCD.OOODOQDODQ;
ArrayList arr  = ODODDCCOQO.OCDCQOOODO(false);
for(int i = 0; i < activeODODDQQO.Length; i++){  
ODODDQQO so = (ODODDQQO)arr[activeODODDQQO[i]];

GameObject goi  = null;
if(so.OQDDQOODCQ != "") goi =  (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.OQDDQOODCQ), typeof(GameObject));
GameObject OQQDCDCOQO = null;
if(so.ODDDOCCOQO != "") OQQDCDCOQO = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.ODDDOCCOQO), typeof(GameObject));
GameObject OOQQOCOOQC = null;
if(so.ODOQDQQCCQ != "") OOQQOCOOQC =  (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.ODOQDQQCCQ), typeof(GameObject));
ODODDCCOQO.OCCCOOOCOC(so, pnts, obj, OOQQCODOCD, param, OOQDOOQQ, activeODODDQQO[i], raise, goi, OQQDCDCOQO, OOQQOCOOQC);
if(so.terrainTree > 0){

if(EditorUtility.DisplayDialog("Side Objects", "Side Object " + so.name + " in " + target.gameObject.name + " includes an asset part of the terrain vegetation data.\n\nWould you like to add this side object to the terrain vegetation data?", "yes","no")){
foreach(Transform child in mySideObject){
if(child.gameObject.name == so.name){
ODODDCCOQO.OOQOQOOCQD(activeODODDQQO[i], child);
MonoBehaviour.DestroyImmediate(child.gameObject);
break;
}
}
}
}
foreach(Transform child in mySideObject)if(child.gameObject.GetComponent(typeof(sideObjectScript)) != null) MonoBehaviour.DestroyImmediate(child.gameObject.GetComponent(typeof(sideObjectScript)));
}
}

static public void OCOOCOOQOD(sideObjectScript scr, int index, RoadObjectScript target, Transform go){
string n = go.gameObject.name;
Transform p = go.parent;

if(go != null){
MonoBehaviour.DestroyImmediate(go.gameObject);
}
ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
ODODDQQO so = (ODODDQQO)arr[index];

OCQCQQDCQC(n, p, so, index, target);

GameObject goi  = null;
if(so.OQDDQOODCQ != "") goi =  (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.OQDDQOODCQ), typeof(GameObject));
GameObject OQQDCDCOQO = null;
if(so.ODDDOCCOQO != "") OQQDCDCOQO = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.ODDDOCCOQO), typeof(GameObject));
GameObject OOQQOCOOQC = null;
if(so.ODOQDQQCCQ != "") OOQQOCOOQC =  (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.ODOQDQQCCQ), typeof(GameObject));

ODODDCCOQO.OCCCDOCDCO(target.OOQQCODOCD, target.transform, target.ODCOQCODCC(), target.OOQDOOQQ, index, target.raise, goi, OQQDCDCOQO, OOQQOCOOQC);
arr = null;
}

static public Transform OQQQQDQCCO(RoadObjectScript target){

GameObject go  =  new GameObject("Side Objects");

go.transform.parent = target.transform;
ArrayList arr = ODODDCCOQO.OCDCQOOODO(false);
for(int i = 0; i < target.OOQQQOQO.Length; i++){  
ODODDQQO so = (ODODDQQO)arr[target.OOQQQOQO[i]];
OCQCQQDCQC(so.name, go.transform, so, target.OOQQQOQO[i], target);
}
return go.transform;
}
static public void OCQCQQDCQC(string objectname, Transform obj, ODODDQQO so, int index, RoadObjectScript target){



Transform rootObject = null;

foreach(Transform child1 in obj)
{
if(child1.name == objectname){
rootObject = child1;

if(so.textureGUID !=""){
MeshRenderer mr  = (MeshRenderer)rootObject.transform.GetComponent(typeof(MeshRenderer));
Material mat =  (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.textureGUID), typeof(Material));
mr.material = mat;

}
}
}
if(rootObject == null){
GameObject go  =  new GameObject(objectname);
go.name = objectname;
go.transform.parent = obj;
rootObject = go.transform;

go.AddComponent(typeof(MeshFilter));
go.AddComponent(typeof(MeshRenderer));
go.AddComponent(typeof(MeshCollider));
go.AddComponent(typeof(sideObjectScript));
sideObjectScript scr = (sideObjectScript)go.GetComponent(typeof(sideObjectScript));
if(so.textureGUID !=""){
MeshRenderer mr = (MeshRenderer)go.GetComponent(typeof(MeshRenderer));
Material mat =  (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(so.textureGUID), typeof(Material));
mr.material = mat;
scr.mat = mat;
}
scr.soIndex = index;
scr.soName = so.name;

scr.soAlign = int.Parse(so.align);
scr.soUVx = so.uvx;
scr.soUVy = so.uvy;
scr.m_distance = so.m_distance;
scr.objectType = so.objectType;
scr.weld = so.weld;
scr.combine = so.combine;
scr.OQCCQQDDOC = so.OQCCQQDDOC;
scr.m_go = so.OQDDQOODCQ;
if(so.ODDDOCCOQO != ""){
scr.ODDDOCCOQO = so.ODDDOCCOQO;

}
if(so.ODDDOCCOQO != ""){
scr.ODOQDQQCCQ = so.ODOQDQQCCQ;

}
scr.selectedRotation = so.selectedRotation;
scr.position = so.position;
scr.uvInt = so.uvType;
scr.randomObjects = so.randomObjects;
scr.childOrder = so.childOrder;
scr.sidewaysOffset = so.sidewaysOffset;
scr.density = so.density;
scr.OODCCOODCC = target;
scr.terrainTree = so.terrainTree;
scr.xPosition = so.xPosition;
scr.yPosition = so.yPosition;
scr.uvYRound = so.uvYRound;
scr.m_collider = so.collider;

}
}

}
