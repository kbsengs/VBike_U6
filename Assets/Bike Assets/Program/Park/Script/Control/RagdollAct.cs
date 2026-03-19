using UnityEngine;
using System.Collections;

public class RagdollAct : MonoBehaviour {

    public int number;
    private float power = 50;
    public Renderer[] renderer;
    public Transform cameraPoint;

    void Start () {
        transform.tag = "Player";
        Destroy(gameObject, 3.0f);
    }

    public void Act(float speed)
    {
        renderer = GetComponentsInChildren<Renderer>();

        foreach (Renderer pos in renderer)
        {
            pos.material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Cycles_Challenge/" + (number + 1).ToString()));
        }

        Vector3 dir = new Vector3(transform.forward.x * power * speed, transform.forward.x * 10, transform.forward.z * power * speed);
        Rigidbody[] all = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody pos in all)
        {
            pos.AddForce(dir);
        }
        //print((power * speed).ToString());
        //if (DataInfo.GameKind != "Auto")
        //{
        AudioSource.PlayClipAtPoint(AudioCtr.snd_crash[Random.Range(0, 3)], transform.position);
        AudioSource.PlayClipAtPoint(AudioCtr.snd_crash_voice[Random.Range(0, 6)], transform.position);
        //}
    }
}
