using UnityEngine;

public class BatFace : MonoBehaviour
{
    private Vector3 origin;

    private void Start()
    {
        origin = transform.localPosition;
    }

    public void LookAt(Vector3 pos, float amount = 0.2f)
    {
        var dir = (pos - transform.position).normalized * amount;
        Tweener.Instance.MoveLocalTo(transform, dir, Random.Range(0.15f, 0.3f), Random.Range(0f, 0.2f),
            TweenEasings.BounceEaseOut);
    }
}