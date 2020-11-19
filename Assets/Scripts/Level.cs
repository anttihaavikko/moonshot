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
        levels.moon.SetGuns(hasLeftGun, hasRightGun);
        levels.moon.SetLevel(this);
        gameObject.SetActive(true);
        levels.backdrop.position = transform.position;
        levels.moon.transform.position = spawn.position;
        info = levels.GetInfo(index);

        this.StartCoroutine(() => levels.levelInfo.Show(info.name, info.description), 0.6f);
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
        }

        return false;
    }

    public void AddShot()
    {
        shotCount++;
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
    Extra
}
