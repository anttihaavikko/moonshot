using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndView : MonoBehaviour
{
    public Transform camRig;
    public Zoomer zoomer;
    
    // Start is called before the first frame update
    private void Start()
    {
        AudioManager.Instance.ChangeMusic(1, 0.5f, 0.5f, 0f);
        zoomer.ZoomTo(9f);
        Tweener.Instance.MoveTo(camRig, new Vector3(0f, -16f, 0f), 1f, 0f, TweenEasings.BounceEaseOut);
    }
}
