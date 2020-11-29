using UnityEngine;

public class RandomAnimation : MonoBehaviour
{
    public Animator anim;

    // Start is called before the first frame update
    private void Start()
    {
        anim.speed = Random.Range(0.9f, 1.1f);
    }
}