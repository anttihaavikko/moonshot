using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        transform.position = target.position;
    }
}