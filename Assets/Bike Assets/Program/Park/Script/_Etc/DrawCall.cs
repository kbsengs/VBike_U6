using UnityEngine;
using System.Collections;

public class DrawCall : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Merge();
	}

    public void Merge()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        //meshFilter.mesh.Clear();

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        transform.GetComponent<MeshRenderer>().material =
              meshFilters[0].GetComponent<Renderer>().sharedMaterial; // Unity6 Migration

        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int i = 0;
        int ci = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilter != meshFilters[i])
            {
                combine[ci].mesh = meshFilters[i].sharedMesh;
                combine[ci].transform = meshFilters[i].transform.localToWorldMatrix;
                ++ci;
            }
            meshFilters[i].gameObject.SetActive(false); // Unity6 Migration
            i++;
        }
        meshFilter.mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true); // Unity6 Migration

        transform.gameObject.GetComponent<MeshCollider>().sharedMesh =
        transform.gameObject.GetComponent<MeshFilter>().mesh;
    }
}
