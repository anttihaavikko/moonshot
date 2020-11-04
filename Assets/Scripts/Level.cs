using UnityEngine;

public abstract class Level : MonoBehaviour
{
    public Levels levels;
    public Transform spawn;

    public bool hasLeftGun = true;
    public bool hasRightGun = true;

    public void Complete()
    {
        levels.ChangeLevel(1);
    }

    public virtual void Activate()
    {
        levels.moon.SetGuns(hasLeftGun, hasRightGun);
        levels.moon.SetLevel(this);
        gameObject.SetActive(true);
        levels.backdrop.position = transform.position;
        levels.moon.transform.position = spawn.position;
    }

    public Vector3 GetCamPos()
    {
        return new Vector3(transform.position.x, transform.position.y, -10f);
    }
}