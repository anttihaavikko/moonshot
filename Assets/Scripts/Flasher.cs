using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flasher : MonoBehaviour
{
    public List<SpriteRenderer> darkSprites, lightsSprites;

    public void Flash()
    {
        darkSprites.ForEach(s => s.color = Color.white);
        lightsSprites.ForEach(s => s.color = Color.black);

        CancelInvoke("UnFlash");
        Invoke("UnFlash", 0.1f);
    }

    public void UnFlash()
    {
        darkSprites.ForEach(s => s.color = Color.black);
        lightsSprites.ForEach(s => s.color = Color.white);
    }
}
