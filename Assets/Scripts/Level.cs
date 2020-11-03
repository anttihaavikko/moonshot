﻿using UnityEngine;

public abstract class Level : MonoBehaviour
{
    public Levels levels;
    public Transform spawn;

    public void Complete()
    {

    }

    public virtual void Activate()
    {
        gameObject.SetActive(true);
        levels.backdrop.position = transform.position;
        levels.moon.transform.position = spawn.position;
    }

    public Vector3 GetCamPos()
    {
        return new Vector3(transform.position.x, transform.position.y, -10f);
    }
}