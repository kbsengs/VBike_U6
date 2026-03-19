using UnityEngine;
using System.Collections;

public class Cycle_Animation : MonoBehaviour {

    private Cycle_Control _control;
    //private AnimationState foot;
    private AnimationState left;
    private AnimationState right;
    //private AnimationState body;
    private AnimationState normal;

    private AnimationState up;
    private AnimationState down;
    private AnimationState kick_Left;
    private AnimationState kick_Right;

    private AnimationState jump_1;
    private AnimationState jump_2;

    void SetAnimationState(AnimationState ani, AnimationBlendMode blendMode, WrapMode wrapMode, int layer, float weight, bool enable)
    {
        ani.blendMode = blendMode;
        ani.wrapMode = wrapMode;
        ani.layer = layer;
        ani.weight = weight;
        ani.enabled = enable;
    }

    // Use this for initialization
    void Start()
    {
        _control = GetComponent<Cycle_Control>();

        GetComponent<Animation>().Stop();

        GetComponent<Animation>().wrapMode = WrapMode.Loop;

        //foot = transform.animation["foot"];
        left = GetComponent<Animation>()["left"];
        right = GetComponent<Animation>()["right"];
        //body = transform.animation["body"];
        normal = GetComponent<Animation>()["normal"];

        up = GetComponent<Animation>()["up"];
        down = GetComponent<Animation>()["down"];

        kick_Left = GetComponent<Animation>()["Kicking_left(new)"];
        kick_Right = GetComponent<Animation>()["Kicking_right(new)"];

        jump_1 = GetComponent<Animation>()["Jump_fall"];
        jump_2 = GetComponent<Animation>()["Jump_stop"];

        SetAnimationState(left, AnimationBlendMode.Additive, WrapMode.ClampForever, 100, 0, true);
        SetAnimationState(right, AnimationBlendMode.Additive, WrapMode.ClampForever, 100, 0, true);
        SetAnimationState(normal, AnimationBlendMode.Blend, WrapMode.Loop, 200, 0, true);
        SetAnimationState(up, AnimationBlendMode.Blend, WrapMode.Loop, 200, 0, true);
        SetAnimationState(down, AnimationBlendMode.Blend, WrapMode.Loop, 200, 0, true);

        SetAnimationState(kick_Left, AnimationBlendMode.Blend, WrapMode.Once, 300, 0, true);
        SetAnimationState(kick_Right, AnimationBlendMode.Blend, WrapMode.Once, 300, 0, true);

        SetAnimationState(jump_1, AnimationBlendMode.Blend, WrapMode.Once, 250, 0, true);
        SetAnimationState(jump_2, AnimationBlendMode.Blend, WrapMode.Once, 250, 0, true);



        GetComponent<Animation>().SyncLayer(0);

        GetComponent<Animation>()["idle"].layer = -1;
        GetComponent<Animation>()["idle"].enabled = true;
        GetComponent<Animation>()["idle"].weight = 1;
    }


    bool jump = true;
    // Update is called once per frame
    void Update()
    {
        float pedalSpeed = _control.moveValue.pedalSpeed / 25.0f;
        if (!_control.moveValue.groundF && !_control.moveValue.groundR)
            pedalSpeed = 0;
        if (pedalSpeed > 3.5f) pedalSpeed = 3.5f;
        normal.speed = pedalSpeed;
        up.speed = pedalSpeed;
        down.speed = pedalSpeed;

        if (_control.moveValue.realSpeed > 1.0f)// || _control.moveValue.pedalSpeed > 0)
        {
            if (_control.moveValue.realSpeed > 20.0f)
            {
                if (_control.moveValue.heightAngle > 5.0f)
                {
                    normal.weight = Mathf.MoveTowards(normal.weight, 0.0f, 3.0f * Time.deltaTime);
                    up.weight = Mathf.MoveTowards(up.weight, 1.0f, 3.0f * Time.deltaTime);
                    down.weight = Mathf.MoveTowards(down.weight, 0.0f, 3.0f * Time.deltaTime);
                }
                else
                {
                    normal.weight = Mathf.MoveTowards(normal.weight, 0.0f, 3.0f * Time.deltaTime);
                    up.weight = Mathf.MoveTowards(up.weight, 0.0f, 3.0f * Time.deltaTime);
                    down.weight = Mathf.MoveTowards(down.weight, 1.0f, 3.0f * Time.deltaTime);
                }
            }
            else
            {
                normal.weight = Mathf.MoveTowards(normal.weight, 1.0f, 3.0f * Time.deltaTime);
                up.weight = Mathf.MoveTowards(up.weight, 0.0f, 3 * Time.deltaTime);
                down.weight = Mathf.MoveTowards(down.weight, 0.0f, 3.0f * Time.deltaTime);
            }
        }
        else
        {
            normal.weight = Mathf.MoveTowards(normal.weight, 0, 3 * Time.deltaTime);
            up.weight = Mathf.MoveTowards(up.weight, 0, 3 * Time.deltaTime);
            down.weight = Mathf.MoveTowards(down.weight, 0, 3 * Time.deltaTime);
        }

        float angle = _control.moveValue.steer / (35 * 10.0f);

        if (angle != 0)
        {
            if (angle > 0)
            {
                left.normalizedTime = 0;
                right.normalizedTime = angle;
            }
            else if (angle < 0)
            {
                left.normalizedTime = -angle;
                right.normalizedTime = 0;
            }
            
            left.weight = 1;
            right.weight = 1;
        }
        else
        {
            if (left.weight > 0)
                left.weight -= 5.0f * Time.deltaTime;
            if (right.weight > 0)
                right.weight -= 5.0f * Time.deltaTime;
        }

        if (!_control.moveValue.groundF && !_control.moveValue.groundR && !jump)
        {
            //animation.CrossFade(jump_1.name);
            jump = true;
            //animation.Play("Jump_fall");
        }

        if ((_control.moveValue.groundF || _control.moveValue.groundR) && jump)
        {
            GetComponent<Animation>().CrossFade(jump_2.name);
            jump = false;
            //print("jump");
            //animation.Play("Jump_stop");
        }

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    animation.CrossFade("Kicking_left");
        //}
        //else if (Input.GetKeyDown(KeyCode.E))
        //{
        //    animation.CrossFade("Kicking_right");
        //}

    }
}
