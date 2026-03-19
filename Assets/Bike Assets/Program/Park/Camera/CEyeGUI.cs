using UnityEngine;
using System.Collections;

public class CEyeGUI : MonoBehaviour {

    private Camera camera_Left;
    private Camera camera_Right;

    public bool two;
    public float eye = 0.1f;
    // Use this for initialization
    void Start()
    {
        if (!GameData._3D)
            two = false;
        else two = true;
        Init();
    }

    public void Set3D()
    {
        if (!GameData._3D)
            two = false;
        else two = true;
        Init();
    }

    void Init()
    {
        camera_Left = GameObject.Find(gameObject.name + "/Left").GetComponent<Camera>();
        camera_Right = GameObject.Find(gameObject.name + "/Right").GetComponent<Camera>();

        camera_Left.aspect = camera_Right.aspect = 2.0f;
        if (!two)
        {
            camera_Right.gameObject.SetActive(false);
            camera_Left.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
        }
        SetCamera();
    }


    void SetCamera()
    {
        if (!two) return;
        camera_Right.transform.localPosition = new Vector3(-eye, 0, 0);
        camera_Left.transform.localPosition = new Vector3(eye, 0, 0);
    }
}
