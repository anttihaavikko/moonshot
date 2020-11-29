using UnityEngine;

public class Mover : MonoBehaviour
{
    public float speed = 1f;
    public float offset;
    public bool noNegatives;
    public Vector3 direction = Vector3.up;

    private Vector3 originalPosition;

    // Use this for initialization
    private void Start()
    {
        SetOrigin();
    }

    // Update is called once per frame
    private void Update()
    {
        var sinVal = Mathf.Sin(Time.time * speed + offset * Mathf.PI);
        sinVal = noNegatives ? Mathf.Abs(sinVal) : sinVal;
        transform.localPosition = originalPosition + direction * sinVal;
    }

    public void SetOrigin()
    {
        originalPosition = transform.localPosition;
    }
}