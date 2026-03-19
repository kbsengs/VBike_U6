using UnityEngine;
using UnityEditor;

// /////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Batch audio import settings modifier.
//
// Modifies all selected audio clips in the project window and applies the requested modification on the
// audio clips. Idea was to have the same choices for multiple files as you would have if you open the
// import settings of a single audio clip. Put this into Assets/Editor and once compiled by Unity you find
// the new functionality in Custom -> Sound. Enjoy! :-)
//
// April 2010. Based on Martin Schultz's texture import settings batch modifier.
//
// /////////////////////////////////////////////////////////////////////////////////////////////////////////
public class ChangeMeshRender : ScriptableObject
{

    [MenuItem("Custom/MeshRender/Off")]
    static void ToggleMeshRender_Disable()
    {
        SelectedToggleMeshRender(false);
    }

    [MenuItem("Custom/MeshRender/On")]
    static void ToggleMeshRender_Enable()
    {
        SelectedToggleMeshRender(true);
    }
	
	static void SelectedToggleMeshRender( bool enable )
	{
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }
		
		for (int i = 0; i < selection.Length; i++)
        {
            Component[] renderer = selection[i].GetComponentsInChildren(typeof(Renderer));

            foreach( Component p in  renderer)
			{
				p.GetComponent<Renderer>().enabled = enable;
			}
        }
	}
	
    static Object[] GetSelectedAudioclips()
    {
        return Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
    }
}