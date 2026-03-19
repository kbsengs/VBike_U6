using UnityEngine;
using UnityEngine.UI; // Unity6 Migration: RawImage -> RawImage
using System.Collections;

public class Training_GUI : MonoBehaviour {

    public float calorie;
    public float distance;
    public float time;
    public float speed;

    private Renderer kcal;
    private Renderer speedR;
    private RawImage[] m_time = new RawImage[4];

    //private Transform time;

    public GameObject _Result;
    public MeshRenderer[] _Result_Time;
    public MeshRenderer[] _Result_Distance;
    public MeshRenderer[] _Result_Claorie;

    public MeshRenderer[] _Time;

    private string numRoot1 = "Textures/d_num_";
    private string numRoot2 = "Textures/s_num_";
    private string numRoot3 = "Textures/t_num_";
    private string numRoot4 = "Textures/r_num_";
    // Use this for initialization
    void Start()
    {
        kcal = transform.Find("kcal/Dummy001").GetComponent<Renderer>();
        speedR = transform.Find("speed/Dummy001").GetComponent<Renderer>();
        //time = transform.Find("time_limit/Object001");
        timeover = transform.Find("time_over").gameObject;
        _Result.SetActive(false);
        timeover.SetActive(false);
        //m_time[0] = transform.Find("m_time/num_3").RawImage;
        //m_time[1] = transform.Find("m_time/num_2").RawImage;
        //m_time[2] = transform.Find("m_time/num_1").RawImage;
        //m_time[3] = transform.Find("m_time/num_0").RawImage;
    }

    // Update is called once per frame
    void Update()
    {
        ShowKcal();
        ShowDistance();
        //ShowTime();
        ShowSpeed();
        ShowTime();
        //ShowTimeOver();
    }

    float myCalorie;
    void ShowKcal()
    {
        myCalorie += ((0.28f * Mathf.Abs(calorie)) * 70.0f / 3600.0f) * Time.deltaTime;
        int cal = Mathf.Abs((int)(myCalorie * 100));
        int _100 = (int)((cal / 10000) % 10);
        int _10 = (int)((cal / 1000) % 10);
        int _1 = (int)((cal / 100) % 10);
        int _01 = (int)((cal / 10) % 10);
        int _001 = (int)((cal / 1) % 10);

        kcal.materials[1].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _100.ToString()));
        kcal.materials[2].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _10.ToString()));
        kcal.materials[3].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _1.ToString()));
        kcal.materials[4].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _01.ToString()));
        kcal.materials[5].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _001.ToString()));
    }

    float myDistance;
    void ShowDistance()
    {
        myDistance += (Mathf.Abs(distance) / 3.6f) * Time.deltaTime;
        int dis = Mathf.Abs((int)myDistance);
        int _100 = (int)((dis / 10000) % 10);
        int _10 = (int)((dis / 1000) % 10);
        int _1 = (int)((dis / 100) % 10);
        int _01 = (int)((dis / 10) % 10);
        int _001 = (int)((dis / 1) % 10);

        kcal.materials[6].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _100.ToString()));
        kcal.materials[7].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _10.ToString()));
        kcal.materials[8].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _1.ToString()));
        kcal.materials[9].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _01.ToString()));
        kcal.materials[10].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _001.ToString()));
    }

    //int aninum;
    //void ShowTime()
    //{
    //    int t = Mathf.Abs((int)time);
    //    int m = (int)(t / 60);
    //    int s = t - m * 60;
    //    int m_10 = (int)((m / 10) % 10);
    //    int m_1 = (int)((m / 1) % 10);
    //    int s_10 = (int)((s / 10) % 10);
    //    int s_1 = (int)((s / 1) % 10);
    //    if (s_1 != aninum)
    //    {
    //        m_time[3].GetComponent<Animation>().Stop();
    //        m_time[3].GetComponent<Animation>().Play();
    //    }
    //    aninum = s_1;

    //    m_time[0].texture = (Texture)Resources.Load(numRoot3 + m_10.ToString());
    //    m_time[1].texture = (Texture)Resources.Load(numRoot3 + m_1.ToString());
    //    m_time[2].texture = (Texture)Resources.Load(numRoot3 + s_10.ToString());
    //    m_time[3].texture = (Texture)Resources.Load(numRoot3 + s_1.ToString());
    //}

    float angle;
    void ShowSpeed()
    {
        int s = Mathf.Abs((int)speed);
        int _100 = (int)((s / 100) % 10);
        int _10 = (int)((s / 10) % 10);
        int _1 = (int)((s / 1) % 10);

        Transform t = speedR.transform.Find("Bone001");
        AnimationState state = t.GetComponent<Animation>()[t.GetComponent<Animation>().clip.name];
        state.speed = 2.0f;
        float v = 0.0f;
        float wantangle = 180 + 3.0f * s;
        if (wantangle > 360)
        {
            if (!t.GetComponent<Animation>().isPlaying) t.GetComponent<Animation>().Play();
            wantangle = 360;
        }
        else t.GetComponent<Animation>().Stop();
        angle = Mathf.SmoothDamp(angle, wantangle, ref v, 0.1f);


        t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, angle);

        speedR.materials[1].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _100.ToString()));
        speedR.materials[2].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _10.ToString()));
        speedR.materials[3].SetTexture("_MainTex", (Texture)Resources.Load(numRoot1 + _1.ToString()));
    }
    public float remainTime;
    void ShowTime()
    {
        //float value = remainTime / GameData.TRAINING_TIME;
        //value = Mathf.Clamp(value, 0.0f, 1.0f);
        //time.localScale = new Vector3(value, 1, 1);
        int sec = (int)(remainTime % 60);
        int min = (int)((int)remainTime / 60);
        int s_1 = sec % 10;
        int s_10 = (sec / 10) % 10;
        int m_1 = min % 10;
        int m_10 = (min / 10) % 10;

        _Time[0].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot3 + m_10.ToString()));
        _Time[1].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot3 + m_1.ToString()));
        _Time[2].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot3 + s_10.ToString()));
        _Time[3].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot3 + s_1.ToString()));
    }

    GameObject timeover;
    void ShowTimeOver()
    {
        if (remainTime <= 1)
        {
            if (!timeover.activeSelf)
                timeover.SetActive(true);
        }
        else
        {
            if (timeover.activeSelf)
                timeover.SetActive(false);
        }
    }

    public void ShowResult()
    {
        _Result.SetActive(true);
        myCalorie += ((0.28f * Mathf.Abs(calorie)) * 70.0f / 3600.0f) * Time.deltaTime;
        int cal = Mathf.Abs((int)(myCalorie * 100));
        int c_100 = (int)((cal / 10000) % 10);
        int c_10 = (int)((cal / 1000) % 10);
        int c_1 = (int)((cal / 100) % 10);
        int c_01 = (int)((cal / 10) % 10);
        int c_001 = (int)((cal / 1) % 10);

        _Result_Claorie[0].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + c_100.ToString()));
        _Result_Claorie[1].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + c_10.ToString()));
        _Result_Claorie[2].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + c_1.ToString()));
        _Result_Claorie[3].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + c_01.ToString()));
        _Result_Claorie[4].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + c_001.ToString()));

        int dis = Mathf.Abs((int)myDistance);
        int d_100 = (int)((dis / 10000) % 10);
        int d_10 = (int)((dis / 1000) % 10);
        int d_1 = (int)((dis / 100) % 10);
        int d_01 = (int)((dis / 10) % 10);
        int d_001 = (int)((dis / 1) % 10);

        _Result_Distance[0].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + d_100.ToString()));
        _Result_Distance[1].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + d_10.ToString()));
        _Result_Distance[2].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + d_1.ToString()));
        _Result_Distance[3].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + d_01.ToString()));
        _Result_Distance[4].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + d_001.ToString()));

        int sec = (int)(remainTime % 60);
        int min = (int)((int)remainTime / 60);
        int s_1 = sec % 10;
        int s_10 = (sec / 10) % 10;
        int m_1 = min % 10;
        int m_10 = (min / 10) % 10;

        _Result_Time[0].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + m_10.ToString()));
        _Result_Time[1].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + m_1.ToString()));
        _Result_Time[2].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + s_10.ToString()));
        _Result_Time[3].material.SetTexture("_MainTex", (Texture)Resources.Load(numRoot4 + s_1.ToString()));
    }
}
