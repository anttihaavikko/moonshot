using UnityEngine;

public abstract class Level : MonoBehaviour
{
    public string levelName, levelDesc;
    [TextArea]
    public string message;
    public Levels levels;
    public Transform spawn;
    [TextArea]
    public string winMessage;

    public bool hasLeftGun = true;
    public bool hasRightGun = true;

    private bool completed;

    public void Complete()
    {
        if(!completed)
        {
            completed = true;
            this.StartCoroutine(() => levels.moon.bubble.Show(winMessage, true), 0.2f);
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

        this.StartCoroutine(() => levels.levelInfo.Show(levelName, levelDesc), 0.6f);
    }

    public virtual void AfterInfo()
    {
        levels.moon.bubble.Show(message);
    }

    public Vector3 GetCamPos()
    {
        return new Vector3(transform.position.x, transform.position.y, -10f);
    }

    public abstract void CheckEnd(float time = 0f);
}