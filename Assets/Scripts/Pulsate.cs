using UnityEngine;

public class Pulsate : MonoBehaviour
{
    public float amount = 0.1f;
    public float speed = 1f;
    public bool oneSided;
    public Vector3 ratio = Vector3.one;

    private Vector3 originalScale;

    // Use this for initialization
    private void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    private void Update()
    {
        var amt = Mathf.Sin(Time.time * speed);
        amt = oneSided ? Mathf.Abs(amt) : amt;

        transform.localScale = originalScale + ratio * amount * amt;
    }
}