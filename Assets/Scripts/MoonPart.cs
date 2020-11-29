using UnityEngine;

public class MoonPart : MonoBehaviour
{
    public Moon moon;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy") moon.Die();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy") moon.Die();
    }
}