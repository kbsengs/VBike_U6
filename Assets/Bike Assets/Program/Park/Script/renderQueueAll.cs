using UnityEngine;
using System.Collections;

public class renderQueueAll : MonoBehaviour {

    public int renderOrder_1 = -10;
    public int renderOrder_2 = 0;
    public Renderer[] renderers_1;
    public Renderer[] renderers_2;
	// Use this for initialization
	
	// Update is called once per frame
	void Update () {
        foreach (Renderer pos in renderers_1)
	    {
            pos.material.renderQueue = renderOrder_1;
	    }
        foreach (Renderer pos in renderers_2)
        {
            pos.material.renderQueue = renderOrder_2;
        }
	}
}
