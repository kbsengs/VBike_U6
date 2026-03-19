using UnityEngine;
using System.Collections;

public class zoUVScroll : MonoBehaviour {

    public float scrollSpeed_X = 0.0f;
    public float scrollSpeed_Y = 0.2f;

    float valueX;
    float valueY;

    bool stopX;
    float stopTimeX;
    bool stopY;
    float stopTimeY;
    public float time = 5.0f;
    void Start()
    {
        valueX = GetComponent<Renderer>().material.mainTextureOffset.x;
        valueY = GetComponent<Renderer>().material.mainTextureOffset.y;
    }
	// Update is called once per frame
	void Update () {
        
        if (Mathf.Abs(GetComponent<Renderer>().material.mainTextureOffset.x - valueX) >= 1)
        {
            stopX = true;
            if (scrollSpeed_X >= 0)
                valueX = valueX + 1;
            else valueX = valueX - 1;
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(valueX, GetComponent<Renderer>().material.mainTextureOffset.y);
        }
        if (stopX)
        {
            stopTimeX += Time.deltaTime;
            if (stopTimeX >= time)
            {
                stopX = false;
                stopTimeX = 0;
            }
        }
        else
        {
            float offsetX = GetComponent<Renderer>().material.mainTextureOffset.x;
            offsetX += Time.deltaTime * scrollSpeed_X;

            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offsetX, GetComponent<Renderer>().material.mainTextureOffset.y);
        }

        if (Mathf.Abs(GetComponent<Renderer>().material.mainTextureOffset.y - valueY) >= 1)
        {
            stopY = true;
            if (scrollSpeed_Y >= 0)
                valueY = valueY + 1;
            else valueY = valueY - 1;
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(GetComponent<Renderer>().material.mainTextureOffset.x, valueY);
        }
        if (stopY)
        {
            stopTimeY += Time.deltaTime;
            if (stopTimeY >= time)
            {
                stopY = false;
                stopTimeY = 0;
            }
        }
        else
        {
            float offsetY = GetComponent<Renderer>().material.mainTextureOffset.y;
            offsetY += Time.deltaTime * scrollSpeed_Y;

            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(GetComponent<Renderer>().material.mainTextureOffset.x, offsetY);
        }
	}
}
