using UnityEngine;
using System.Collections;

public class Mission : MonoBehaviour {

    public static void NetLoadMission(System.String prefebName)
    {
        // Unity6 Migration: Network.Instantiate removed. TODO Phase 4: re-implement via custom UDP
        Debug.Log("NetLoadMission (network disabled): " + prefebName);
    }
    public static void LoadMission(System.String prefebName)
    {
        Debug.Log("Instantiate = " + prefebName);
        Instantiate((GameObject)Resources.Load(prefebName));
    }

    //public static void Load()
    //{
    //    switch (DataInfo.map)
    //    {
    //        case 0: // 
    //            LoadMission("Prefeb_Map1/rollingStone");
    //            LoadMission("Prefeb_Map1/AllSeagull");
    //            break;
    //        case 1:                
    //            LoadMission("Prefeb_Map2/Rolling Barrel1");
    //            LoadMission("Prefeb_Map2/Rolling Barrel2");
    //            break;
    //        case 2:
    //            LoadMission("Prefeb_Map3/Rolling car");
    //            LoadMission("Prefeb_Map3/Rolling tire01");
    //            LoadMission("Prefeb_Map3/Rolling tire02");
    //            break;
    //    }
    //}
}
