using UnityEngine;
using UnityEngine.UI;

public class FlyingText : MonoBehaviour
{
    private float alpha = 1f;
    private readonly float duration = 2f;
    private readonly float speed = 1f;
    private Vector2 startScale;

    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
        startScale = text.transform.localScale;
    }

    private void Update()
    {
        if (alpha > 0)
        {
            // change the y position
            var pos = transform.position;
            pos.y += speed * Time.deltaTime;
            transform.position = pos;

            // change alpha value
            alpha -= Time.deltaTime / duration;

            var color = text.color;
            color.a = alpha;
            text.color = color;

            text.transform.localScale = startScale * (0.5f + 0.5f * alpha);
        }
        else
        {
            // destroy the game object if it's invisible
            Destroy(gameObject);
        }
    }
}