using UnityEngine;

public class Platform : MonoBehaviour
{
    public Transform start, end;
    public Vector3 direction;

    // Update is called once per frame
    private void Update()
    {
        transform.position += direction * Time.deltaTime;

        if (transform.position.y > end.position.y && direction.y > 0 ||
            transform.position.y < end.position.y && direction.y < 0)
            transform.position = new Vector3(transform.position.x, start.position.y, 0);
    }
}