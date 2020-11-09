using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleTrigger : MonoBehaviour
{
    [TextArea]
    public string message;
    [TextArea]
    public string mobileMessage;
    public float delay;
    [HideInInspector]
    public bool shown;

    public string GetMessage()
    {
        return Application.isMobilePlatform ? mobileMessage : message;
    }
}
