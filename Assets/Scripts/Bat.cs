using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour
{
    public Rigidbody2D body;
    public Transform target;
    public float speed = 1f;
    public bool isMoving = true;

    private Vector3 startPos, endPos;
    private bool returning;
    private Vector3 currentTarget;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        endPos = target.position;
        currentTarget = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving) return;

        var dir = currentTarget - transform.position;
        body.AddForce(dir.normalized * speed);

        if((transform.position - currentTarget).magnitude < 0.5f)
        {
            ChangeTarget();
        }
    }

    void ChangeTarget()
    {
        returning = !returning;
        currentTarget = returning ? startPos : endPos;
    }
}
