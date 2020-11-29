using UnityEngine;

public class RandomScale : MonoBehaviour
{
    public float min = 0.9f;
    public float max = 1.1f;

    // Start is called before the first frame update
    private void Start()
    {
        transform.localScale *= Random.Range(min, max);
    }
}