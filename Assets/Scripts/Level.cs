using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Level : MonoBehaviour
{
    public int index;
    public Levels levels;
    public Transform spawn;
    public float zoom = 8.5f;

    public bool hasLeftGun = true;
    public bool hasRightGun = true;

    public LevelBonus[] bonuses;

    private bool completed;
    private LevelData info;
    private int shotCount;
    private float levelTime;
    private bool timeOn;
    private bool bonusTriggered;
    private Queue<string> code;
    private string expectedCode;
    private bool codeComplete;

    public void Restart()
    {
        this.StartCoroutine(() => SceneChanger.Instance.ChangeScene("Main"), 2f);
    }

    public void Complete()
    {
        if(!completed)
        {
            completed = true;
            this.StartCoroutine(() => {
                if(!levels.moon.HasDied())
                {
                    levels.moon.bubble.Show(info.winMessage, true);
                }
            }, 0.2f);

            this.StartCoroutine(() =>
            {
                SaveManager.Instance.CompleteLevel(index);
                levels.levelInfo.ShowEnd();
            }, 1.5f);
        }
    }

    public virtual void Activate()
    {
        code = new Queue<string>();
        levels.moon.SetGuns(hasLeftGun, hasRightGun);
        levels.moon.SetLevel(this);
        gameObject.SetActive(true);
        levels.backdrop.position = transform.position;
        levels.moon.transform.position = spawn.position;
        info = levels.GetInfo(index);

        this.StartCoroutine(() => levels.levelInfo.Show(info.name, info.description), 0.6f);

        var data = SaveManager.Instance.GetDataFor(index);
        for(var i = 0; i < 3; i++)
        {
            if(bonuses.Length >= i + 1)
            {
                if (bonuses[i].type == BonusType.Moon && data.bonusesDone[i])
                {
                    bonuses[i].moon.SetActive(false);
                }

                if (bonuses[i].type == BonusType.Code)
                {
                    expectedCode = bonuses[i].name;
                }
            }
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

    private void Update()
    {
        if(timeOn)
        {
            levelTime += Time.deltaTime;
        }
    }

    public abstract void CheckEnd(float time = 0f);

    public bool IsBonusDone(LevelBonus bonus)
    {
        switch(bonus.type)
        {
            case BonusType.Moon:
                return !bonus.moon.activeSelf;
            case BonusType.Par:
                return shotCount <= bonus.par;
            case BonusType.Time:
                print(levelTime + " vs " + bonus.time);
                return levelTime <= bonus.time;
            case BonusType.Extra:
                return levels.moon.GetTime() >= bonus.time;
            case BonusType.Trigger:
                return bonusTriggered;
            case BonusType.Code:
                return codeComplete;
        }

        return false;
    }
     
    public void AddShot(string letter)
    {
        code.Enqueue(letter);
        if (expectedCode != null && code.Count > expectedCode.Length)
            code.Dequeue();
        shotCount++;
        codeComplete |= string.Join("", code) == expectedCode;
    }

    public void TriggerBonus()
    {
        print("Triggered bonus event!");
        bonusTriggered = true;
    }
}

[System.Serializable]
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
    Code
}
