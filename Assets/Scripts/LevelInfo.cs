using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public List<Appearer> appearers;
    public Levels levels;
    public bool closeWithAny = true;
    public List<TickBox> bonuses;

    public TMPro.TMP_Text nameText, descText, nameShadow, descShadow;

    private bool shown;
    private bool clickDisabled;
    private bool infoShown;

    public void Show()
    {
        shown = true;
        appearers.ForEach(a => a.ShowAfterDelay());

        var bonusNames = levels.GetCurrentLevel().bonuses;

        for(var i = 0; i < bonusNames.Length; i++)
        {
            bonuses[i].text.text = bonusNames[i].name;
        }
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

            if(!infoShown)
            {
                infoShown = true;
                levels.AfterInfo();
            }
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

    public void ShowEnd()
    {
        clickDisabled = true;

        Show();

        var level = levels.GetCurrentLevel();
        var bonusesDone = level.bonuses.Select(level.IsBonusDone).ToList();
        for (var i = 0; i < bonusesDone.Count(); i++)
        {
            if(bonusesDone[i])
            {
                var idx = i;
                this.StartCoroutine(() => bonuses[idx].Check(), 1f + 0.1f * i);
            }
        }
    }
}
