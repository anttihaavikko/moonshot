using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public List<Appearer> appearers;
    public Levels levels;

    public TMPro.TMP_Text nameText, descText, nameShadow, descShadow;

    private bool shown;

    public void Show(string levelName, string description)
    {
        shown = true;
        nameText.text = nameShadow.text = levelName;
        descText.text = descShadow.text = description;
        appearers.ForEach(a => a.ShowAfterDelay());
    }

    public void Hide()
    {
        appearers.ForEach(a => a.Hide());
        this.StartCoroutine(() => {
            shown = false;
            levels.AfterInfo();
        }, 0.3f);
    }

    private void Update()
    {
        if(shown && Input.anyKeyDown)
        {
            Hide();
        }
    }

    public bool IsShown()
    {
        return shown;
    }
}
