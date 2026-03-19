using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WaypointDefine : MonoBehaviour {

    #region Members

    public Transform[] allways;
    public int[] ways = new int[2]; // 0 = start, 1 = end
    public int[] crossStart;

    public int startpoint;

    public int[] crossways_1 = new int[2];// 0 = start, 1 = end
    public int[] crossways_2 = new int[2];// 0 = start, 1 = end
    public int[] crossways_3 = new int[2];// 0 = start, 1 = end
    public int[] crossways_4 = new int[2];// 0 = start, 1 = end
    public int[] crossways_5 = new int[2];// 0 = start, 1 = end

    #endregion

    //void Start()
    //{
        //foreach(Transform pos in allways)
        //{
        //    pos.renderer.enabled = false;
        //}
    //}
}
