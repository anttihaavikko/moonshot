using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickBox : MonoBehaviour
{
    public TMPro.TMP_Text text;
    public Transform mark;

    public void Check()
    {
        Tweener.Instance.ScaleTo(mark, Vector3.one, 0.4f, 0f, TweenEasings.BounceEaseOut);
        Tweener.Instance.RotateTo(mark, Quaternion.Euler(0, 0, -180f), 0.4f, 0f, TweenEasings.BounceEaseOut);
    }
}
