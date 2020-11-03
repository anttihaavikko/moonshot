using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    private float speed;
    private readonly float edge = 20f;

    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(0.5f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime * 0.1f;
        if(transform.localPosition.x >= edge)
        {
            transform.localPosition += Vector3.left * 2f * edge;
        }
    }
}
