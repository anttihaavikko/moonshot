using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toucher : MonoBehaviour
{
    public Moon moon;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "NoTouch")
        {
            moon.Touched();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "NoTouch")
        {
            moon.Touched();
        }
    }
}
