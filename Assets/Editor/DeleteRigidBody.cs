using UnityEngine;
using UnityEditor;
using System.Collections;

public class DeleteRigidBody : EditorWindow
{

    [MenuItem("Custom/DeleteRigidBody")]
    static void SelectChangeTagName()
    {
        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        Debug.Log("Start!");
        if (selection.Length == 0)
        {
            Debug.Log("None Selection");
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }
        //Component[] tran = selection[0].GetComponentsInChildren(typeof(Transform));

        //string TagName = tran[0].gameObject.tag;
        //Debug.Log(TagName);

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] trans = selection[i].GetComponentsInChildren(typeof(Transform));

            foreach (Component p in trans)
            {
                DestroyImmediate(p.GetComponent<Rigidbody>());
                Debug.Log(i + " 번 RigidBody 제거 완료.");
            }
        }
        Debug.Log("DeleteRigidBody 완료.");
    }
}
