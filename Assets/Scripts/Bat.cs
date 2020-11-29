using UnityEngine;

public class Bat : MonoBehaviour
{
    public Rigidbody2D body;
    public Transform target;
    public float speed = 1f;
    public bool isMoving = true;
    private Vector3 currentTarget;
    private bool returning;

    private Vector3 startPos, endPos;

    // Start is called before the first frame update
    private void Start()
    {
        startPos = transform.position;
        endPos = target.position;
        currentTarget = target.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isMoving) return;

        var dir = currentTarget - transform.position;
        body.AddForce(dir.normalized * speed);

        if ((transform.position - currentTarget).magnitude < 0.5f) ChangeTarget();
    }

    private void ChangeTarget()
    {
        returning = !returning;
        currentTarget = returning ? startPos : endPos;
    }
}