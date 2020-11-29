using UnityEngine;

public class Cloud : MonoBehaviour
{
    private readonly float edge = 20f;
    private float speed;

    // Start is called before the first frame update
    private void Start()
    {
        speed = Random.Range(0.5f, 2f);
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime * 0.1f;
        if (transform.localPosition.x >= edge) transform.localPosition += Vector3.left * 2f * edge;
    }
}