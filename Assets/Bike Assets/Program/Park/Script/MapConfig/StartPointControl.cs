using UnityEngine;
using System.Collections;

public class StartPointControl : MonoBehaviour {

    public Vector3 [] position;
    public Transform[] startPoint;

    public bool randomArray = false;

    public int[] num;

    public void Init()
    {
        Transform[] sp = GetComponentsInChildren<Transform>();
        position = new Vector3[GameData.MAX_PLAYER];
        startPoint = new Transform[GameData.MAX_PLAYER];
        num = new int[GameData.MAX_PLAYER];
        for (int i = 0; i < position.Length; i++)
        {
            startPoint[i] = sp[i + 1];
            position[i] = sp[i + 1].position;
        }
    }

    public void RandomPos()
    {
        for (int i = 0; i < num.Length; i++)
        {
            int temp = (int)Random.Range(0, 10);
            for (int j = 0; j < i; j++ )
            {
                if (num[j] == temp)
                {
                    temp = -1;
                }
            }
            if (temp != -1)
            {
                num[i] = temp;
            }
            else
            {
                i--;
            }
        }
        for (int i = 0; i < num.Length; i++)
        {
            startPoint[i].position = position[num[i]];
        }
    }

    public void SetPos()
    {
        for (int i = 0; i < num.Length; i++)
        {
            startPoint[i].position = position[num[i]];
        }
    }
}
