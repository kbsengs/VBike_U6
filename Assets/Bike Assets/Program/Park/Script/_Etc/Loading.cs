using UnityEngine;
using System.Collections;

public class Loading : MonoBehaviour {

    public float scrollSpeed_X = 0.1f;
    public float scrollSpeed_Y = 0.0f;

    float time = 0;

    public Material material;
    private int num;

    public enum Game
    {
        BMX, MTB
    }
    public Game game = Game.BMX;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(transform);
        if (game == Game.BMX)
        {
            num = Random.Range(0, 2);
        }
        else
        {
            num = Random.Range(0, 3);
        }
        time = 0;
	}

    //void Update()
    //{
    //    float offsetX = Time.time * scrollSpeed_X;
    //    float offsetY = Time.time * scrollSpeed_Y;
    //    material.mainTextureOffset = new Vector2(offsetX, offsetY);
    //}

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            time += Time.deltaTime;
            float offsetX = time * scrollSpeed_X;
            if (game == Game.MTB)
            {
                if (GameData._3D)
                {
                    Graphics.DrawTexture(new Rect(0, 0, 1920 / 2, 1080), (Texture)Resources.Load("Texture/Loading/loading_" + num));
                    Graphics.DrawTexture(new Rect(1920 / 2, 0, 1920 / 2, 1080), (Texture)Resources.Load("Texture/Loading/loading_" + num));
                }
                else 
                    Graphics.DrawTexture(new Rect(0, 0, 1920, 1080), (Texture)Resources.Load("Texture/Loading/loading_" + num));
            }
            else
            {
                if (GameData._3D)
                {
                    Graphics.DrawTexture(new Rect(0, 0, 1920 / 2, 1080), (Texture)Resources.Load("Texture/Loading/loading_bmx_" + num));
                    Graphics.DrawTexture(new Rect(1920 / 2, 0, 1920 / 2, 1080), (Texture)Resources.Load("Texture/Loading/loading_bmx_" + num));
                }
                else
                    Graphics.DrawTexture(new Rect(0, 0, 1920, 1080), (Texture)Resources.Load("Texture/Loading/loading_bmx_" + num));
            }

            if (GameData._3D)
            {
                Graphics.DrawTexture(new Rect(0, 1080 - 175, 1920 / 2, 46), material.mainTexture, new Rect(offsetX, 0, 1, 1), 0, 0, 0, 0); // material);
                Graphics.DrawTexture(new Rect(1920 / 2, 1080 - 175, 1920 / 2, 46), material.mainTexture, new Rect(offsetX, 0, 1, 1), 0, 0, 0, 0); // material);
            }
            else
                Graphics.DrawTexture(new Rect(0, 1080 - 175, 1920, 46), material.mainTexture, new Rect(offsetX, 0, 1, 1), 0, 0, 0, 0); // material);
        }
    }
}
