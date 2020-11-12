using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public List<Appearer> appearers;
    public Levels levels;
    public bool closeWithAny = true;

    public TMPro.TMP_Text nameText, descText, nameShadow, descShadow;

    private bool shown;
    private bool clickDisabled;

    public void Show()
    {
        shown = true;
        appearers.ForEach(a => a.ShowAfterDelay());
    }

    public void Show(string levelName, string description)
    {
        if (!Manager.Instance.showInfo)
        {
            AfterHide();
            return;
        }

        Manager.Instance.showInfo = false;
        
        nameText.text = nameShadow.text = levelName;
        descText.text = descShadow.text = description;
        Show();
    }

    public void Hide()
    {
        appearers.ForEach(a => a.Hide());
        AfterHide();
    }

    void AfterHide()
    {
        this.StartCoroutine(() => {
            shown = false;
            levels.AfterInfo();
        }, 0.3f);
    }

    private void Update()
    {
        if(shown && Input.anyKeyDown && closeWithAny && !clickDisabled)
        {
            Hide();
        }
    }

    public bool IsShown()
    {
        return shown;
    }

    public void SetClicksDisabled(bool state)
    {
        clickDisabled = state;
    }
}
