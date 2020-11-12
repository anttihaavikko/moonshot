using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartView : MonoBehaviour
{
    private bool disableStart;
    private bool hasStarted;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (disableStart) return;

        if(Input.anyKeyDown && !hasStarted)
        {
            hasStarted = true;
            SceneChanger.Instance.ChangeScene("LevelSelect");
        }
    }

    public void SetClicksDisabled(bool state)
    {
        disableStart = state;
    }
}
