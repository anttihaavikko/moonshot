using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellManager : MyObjectPool<Rigidbody2D>
{
    private Queue<Rigidbody2D> shells;

    private void Start()
    {
        shells = new Queue<Rigidbody2D>();
    }

    public void Add(Vector3 position, Vector3 dir)
    {
        var shell = GetShell();
        shells.Enqueue(shell);
        shell.transform.position = position;
        shell.AddForce(dir.normalized * 0.05f, ForceMode2D.Impulse);
        shell.AddTorque(Random.Range(-0.5f, 0.5f));
    }

    Rigidbody2D GetShell()
    {
        if (shells.Count < 20)
            return Get(true);

        return shells.Dequeue();
    }
}