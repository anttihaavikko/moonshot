﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Level : MonoBehaviour
{
    public int index;
    public Levels levels;
    public Transform spawn;
    public float zoom = 8.5f;
    public Rigidbody2D moonConnectJoint;

    public bool hasLeftGun = true;
    public bool hasRightGun = true;

    public LevelBonus[] bonuses;
    public float endDelay = 1.5f;
    private bool bonusTriggered;
    private Queue<string> code;
    private bool codeComplete;

    private bool completed;
    private string expectedCode;
    private bool gotDamaged;
    private LevelData info;
    private bool isBoss, isExtra;
    private float levelTime;
    private int shotCount;

    private bool shotLeft, shotRight;
    private bool timeOn;

    private void Update()
    {
        if (timeOn) levelTime += Time.deltaTime;
    }

    public void Restart()
    {
        this.StartCoroutine(() => SceneChanger.Instance.ChangeScene("Main"), 2f);
    }

    public void Complete()
    {
        if (!completed)
        {
            AudioManager.Instance.PlayEffectAt(28, levels.moon.transform.position, 3f);

            completed = true;
            this.StartCoroutine(() =>
            {
                if (!levels.moon.HasDied()) levels.moon.bubble.Show(info.winMessage, true);
            }, 0.2f);

            Invoke("ShowEnd", endDelay);

            // print("Level completed in: " + levelTime + " (" + shotCount + " shots).");
        }
    }

    public void CancelEnd()
    {
        CancelInvoke("ShowEnd");
    }

    private void ShowEnd()
    {
        SaveManager.Instance.CompleteLevel(index);
        if (HasBonuses())
        {
            levels.levelInfo.ShowEnd();   
        }
        else
        {
            SceneChanger.Instance.ChangeScene(isBoss ? "Ultimatum" : "End");
        }
    }

    public bool HasBonuses()
    {
        return !isBoss && !isExtra;
    }

    public virtual void Activate()
    {
        code = new Queue<string>();
        levels.moon.SetGuns(hasLeftGun, hasRightGun);
        if (!hasLeftGun && !hasRightGun)
        {
            levels.moon.DoNoGuns();
        }
        levels.moon.SetLevel(this);
        gameObject.SetActive(true);
        levels.backdrop.position = transform.position;
        levels.moon.transform.position = spawn.position;
        info = levels.GetInfo(index);

        if (moonConnectJoint)
        {
            levels.moon.attachJoint.enabled = true;
            levels.moon.attachJoint.connectedBody = moonConnectJoint;
        }

        levels.levelInfo.SetNames(info.name, info.description);

        this.StartCoroutine(() => levels.levelInfo.Show(info.name, info.description), 0.6f);

        var data = SaveManager.Instance.GetDataFor(index);
        isBoss = info.boss;
        isExtra = info.hidden;
        for (var i = 0; i < 3; i++)
            if (bonuses.Length >= i + 1)
            {
                if (bonuses[i].type == BonusType.Moon && data.bonusesDone[i]) bonuses[i].moon.SetActive(false);

                if (bonuses[i].type == BonusType.Code) expectedCode = bonuses[i].name;
            }
    }

    public virtual void AfterInfo()
    {
        if (levels.moon.HasDied()) return;
        levels.moon.bubble.Show(info.message);
        timeOn = true;
    }

    public Vector3 GetCamPos()
    {
        return new Vector3(transform.position.x, transform.position.y, -10f);
    }

    public abstract void CheckEnd(float time = 0f);

    public bool IsBonusDone(LevelBonus bonus)
    {
        switch (bonus.type)
        {
            case BonusType.Moon:
                return !bonus.moon.activeSelf;
            case BonusType.Par:
                return shotCount <= bonus.par;
            case BonusType.Time:
                return levelTime <= bonus.time;
            case BonusType.Extra:
                return levels.moon.GetTime() >= bonus.time;
            case BonusType.Trigger:
                return bonusTriggered;
            case BonusType.Code:
                return codeComplete;
            case BonusType.LeftOnly:
                return !shotRight;
            case BonusType.RightOnly:
                return !shotLeft;
            case BonusType.Flawless:
                return !gotDamaged;
        }

        return false;
    }

    public void GotDamaged()
    {
        gotDamaged = true;
    }

    public void AddShot(string letter)
    {
        code.Enqueue(letter);
        if (expectedCode != null && code.Count > expectedCode.Length)
            code.Dequeue();
        shotCount++;
        codeComplete |= string.Join("", code) == expectedCode;

        shotLeft |= letter == "L";
        shotRight |= letter == "R";
    }

    public void TriggerBonus()
    {
        print("Triggered bonus event!");
        bonusTriggered = true;
    }
}

[Serializable]
public struct LevelBonus
{
    public BonusType type;
    public string name;
    public int par;
    public float time;
    public GameObject moon;
}

public enum BonusType
{
    None,
    Moon,
    Par,
    Time,
    Extra,
    Trigger,
    Code,
    LeftOnly,
    RightOnly,
    Flawless
}