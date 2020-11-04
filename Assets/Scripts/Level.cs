using UnityEngine;

public abstract class Level : MonoBehaviour
{
    public string levelName, levelDesc;
    [TextArea]
    public string message;
    public Levels levels;
    public Transform spawn;

    public bool hasLeftGun = true;
    public bool hasRightGun = true;

    private bool completed;

    public void Complete()
    {
        if(!completed)
        {
            completed = true;
            this.StartCoroutine(() => levels.ChangeLevel(), 1f);
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
        this.StartCoroutine(() => levels.moon.bubble.Show(message), levels.levelInfo.delay + 0.9f);
    }

    public Vector3 GetCamPos()
    {
        return new Vector3(transform.position.x, transform.position.y, -10f);
    }
}