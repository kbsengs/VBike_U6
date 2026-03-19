using UnityEngine;
using System.Collections;

public class MTB_S_Data : MonoBehaviour {

    public int readyTime = 0;
    public float remainTime;
    public Cycle_Control[] ai = new Cycle_Control[GameData.MAX_PLAYER-1];
    public Cycle_Control[] cycles = new Cycle_Control[GameData.MAX_PLAYER];
    public Cycle_Control MyCharacter;

    public RankData rankData;
    public InGameGUI gui;

    public void CreatBike()
    {
        Transform[] startpoint = GameObject.Find("_Startpoint").GetComponentsInChildren<Transform>();
        int count = 0;
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + i), startpoint[i + 1].position, startpoint[i + 1].rotation) as GameObject;
            if (i == GameData.number)
            {
                MyCharacter = obj.GetComponent<Cycle_Control>();
                //MyCharacter.name = "Cycle_User";
                MyCharacter.wayName = "_Waypoint";
                MyCharacter.User = true;
                MyCharacter.cycle_Impact = false;
                MyCharacter.Rb.isKinematic = true;
                MyCharacter.minimapArrow.arrow.localScale = GameData.ArrowSize_MTB[GameData.MTBMap];
                //GameObject.Find("Eye").GetComponent<CycleCam>().SetTarget(MyCharacter.cameraTarget, 1);
                CycleCam cam = FindObjectOfType(typeof(CycleCam)) as CycleCam;
                cam.SetTarget(MyCharacter.cameraTarget, 1);
            }
            else
            {
                ai[count] = obj.GetComponent<Cycle_Control>();                
                ai[count].wayName = "_Waypoint";
                ai[count].User = false;
                //ai[i - inGamePlayer].cycle_AI = true;
                ai[count].cycle_Impact = false;
                ai[count].Rb.isKinematic = true;
                ai[count].minimapArrow.arrow.localScale = GameData.ArrowSize_MTB[GameData.MTBMap];
                count++;
            }
        }
        Destroy(GameObject.Find("_Loading"));
    }

	public int FindFinishPlayer()
    {
        int count = 0;
        for (int i = 0; i < cycles.Length; i++)
        {
            if (cycles[i].gameFinish)
                count++;
        }
        return count;
    }
	
    public void CreatGUI()
    {
        GameObject obj = Instantiate((GameObject)Resources.Load("Game")) as GameObject;
        gui = obj.GetComponent<InGameGUI>();
        obj.name = "_GUI";
    }

    //void OnGUI()
    //{
    //    GUILayout.Label("Map = " + map);
    //    GUILayout.Label("ReadyTime = " + readyTime);
    //}
}
