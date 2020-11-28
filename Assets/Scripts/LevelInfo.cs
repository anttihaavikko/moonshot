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
    public ButtonMenu menu;
    public Appearer completeText;

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

        CheckTickBoxes(false);

        AudioManager.Instance.Highpass(true);
        //AudioManager.Instance.Lowpass(true);
    }

    public void SetNames(string levelName, string description)
    {
        nameText.text = nameShadow.text = levelName;
        descText.text = descShadow.text = description;
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
        completeText.Hide();
        AfterHide();
        AudioManager.Instance.Highpass(false);
        //AudioManager.Instance.Lowpass(false);
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
        menu.buttons[0].gameObject.SetActive(false);
        menu.buttons.RemoveAt(0);
        this.StartCoroutine(menu.ShowEnd, 0.3f);
        this.StartCoroutine(completeText.Show, 0.6f);
        CheckTickBoxes(true);
    }

    void CheckTickBoxes(bool recheck)
    {
        var level = levels.GetCurrentLevel();
        var save = SaveManager.Instance.GetDataFor(level.index);
        var bonusesDone = level.bonuses.Select(level.IsBonusDone).ToList();
        for (var i = 0; i < bonusesDone.Count(); i++)
        {
            if (recheck && bonusesDone[i] || save.bonusesDone[i])
            {
                if(recheck)
                {
                    if(!SaveManager.Instance.CompleteBonus(level.index, i))
                    {
                        var idx = i;
                        bonuses[idx].MarkDone();
                        this.StartCoroutine(() => bonuses[idx].Check(), 1f + 0.1f * idx);
                    }
                }
                else
                {
                    var idx = i;
                    bonuses[idx].Mark();
                }
            }
        }
    }
}
