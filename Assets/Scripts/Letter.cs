using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    public Transform upperFold, lowerFold;
    public Appearer contents;

    private readonly float foldSpeed = 0.4f;
    private readonly float midDelay = 0.1f;

    public void Open()
    {
        Tweener.Instance.ScaleTo(upperFold, new Vector3(1f, -1f, 1f), foldSpeed, 0, TweenEasings.BounceEaseOut);
        Tweener.Instance.MoveLocalTo(upperFold, new Vector3(0f, 0.45f, 0f), foldSpeed, 0, TweenEasings.BounceEaseOut);
        this.StartCoroutine(() =>
        {
            Tweener.Instance.ScaleTo(lowerFold, new Vector3(1f, -1f, 1f), foldSpeed, 0, TweenEasings.BounceEaseOut);
            Tweener.Instance.MoveLocalTo(lowerFold, new Vector3(0f, -0.45f, 0f), foldSpeed, 0, TweenEasings.BounceEaseOut);

            contents.Show();

        }, foldSpeed + midDelay);
    }
}
