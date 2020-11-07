using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Level : MonoBehaviour
{
    public int index;
    public Levels levels;
    public Transform spawn;

    public bool hasLeftGun = true;
    public bool hasRightGun = true;

    private bool completed;
    private LevelData info;

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
            this.StartCoroutine(() => levels.ChangeLevel(), 1.7f);
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
    }

    public Vector3 GetCamPos()
    {
        return new Vector3(transform.position.x, transform.position.y, -10f);
    }

    public abstract void CheckEnd(float time = 0f);
}