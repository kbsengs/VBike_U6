using UnityEngine;
using System.Collections;

public class CAccountTest : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        CAccount.Init();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.C)) CAccount.AddCoin(1);
	}

    void OnGUI()
    {
        for (int i = 0; i < 10; i++)
        {
            string s = CAccount.m_Account[i].date.ToString();
            GUI.Label(new Rect(10, 10 + 20 * i, 100, 20), s);
            s = CAccount.m_Account[i].coin.ToString();
            GUI.Label(new Rect(110, 10 + 20 * i, 100, 20), s);
        }
    }
}
