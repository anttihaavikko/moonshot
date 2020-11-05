using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleTrigger : MonoBehaviour
{
    [TextArea]
    public string message;
    public float delay;
    [HideInInspector]
    public bool shown;
}
