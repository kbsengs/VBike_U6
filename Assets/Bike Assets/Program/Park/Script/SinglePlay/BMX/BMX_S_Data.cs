using UnityEngine;
using System.Collections;

public class BMX_S_Data : MonoBehaviour {

    //public int way = 0;
    public int readyTime = 0;
    //public float remainTime;
    public Cycle_Control[] ai = new Cycle_Control[GameData.MAX_PLAYER-1];
    public Cycle_Control[] cycles = new Cycle_Control[GameData.MAX_PLAYER];
    public Cycle_Control MyCharacter;

    public RankData rankData;
    public InGameGUI gui;

    public void CreatBike()
    {   
        //Transform[] startpoint = GameObject.Find("_Startpoint" + (way + 1).ToString()).GetComponentsInChildren<Transform>();
        Transform[] startpoint = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).GetComponentsInChildren<Transform>();
        gui.Minimap(GameData.BMXMap);
        int count = 0;
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + i), startpoint[i + 1].position, startpoint[i + 1].rotation) as GameObject;
            if (i == GameData.number)
            {
                MyCharacter = obj.GetComponent<Cycle_Control>();
                MyCharacter.wayName = GameData.bmxWay[GameData.BMXMap];
                MyCharacter.User = true;
                MyCharacter.cycle_Impact = false;
                MyCharacter.Rb.isKinematic = true;
                MyCharacter.minimapArrow.arrow.localScale = GameData.ArrowSize_BMX[GameData.BMXMap];
                CycleCam cam = FindObjectOfType(typeof(CycleCam)) as CycleCam;
                cam.SetTarget(MyCharacter.cameraTarget, 1);
                //GameObject.Find("Eye").GetComponent<CycleCam>().SetTarget(MyCharacter.cameraTarget, 1);

                //Debug.Log(MyCharacter.name.ToString());
            }
            else
            {
                ai[count] = obj.GetComponent<Cycle_Control>();
                ai[count].name = "Cycle_" + i + "_AI";
                //ai[count].wayName = "_Waypoint" + (way + 1).ToString();
                ai[count].wayName = GameData.bmxWay[GameData.BMXMap];
                ai[count].User = false;
                //ai[i - inGamePlayer].cycle_AI = true;
                ai[count].cycle_Impact = false;
                ai[count].Rb.isKinematic = true;
                ai[count].minimapArrow.arrow.localScale = GameData.ArrowSize_BMX[GameData.BMXMap];
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
    //    GUILayout.Label("Way = " + (way + 1).ToString());
    //    GUILayout.Label("ReadyTime = " + readyTime);
    //}
}
