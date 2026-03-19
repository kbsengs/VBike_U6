using UnityEngine;
using System.Collections;

public class GameFunctions : MonoBehaviour {

    #region GUI Functions
   
    public float TwoObjectAngle(Vector2 from, Vector2 to, Vector2 forward)
    {
        Vector2 to_Direction = (to - from).normalized;

        float x1 = forward.x;
        float y1 = forward.y;

        float x2 = to_Direction.x;
        float y2 = to_Direction.y;

        float angle = Mathf.Atan2(x2 * y1 - y2 * x1, x2 * x1 + y1 * y2) * Mathf.Rad2Deg;
        return angle;
    }

    public Rect ConvertAxis(float x, float y, float w, float h)
    {
        Rect pos = new Rect(x, 1080 - y - h, w, h);
        return pos;
    }

    #endregion
}
