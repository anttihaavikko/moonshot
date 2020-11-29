using UnityEngine;

public class Shine : MonoBehaviour
{
    public float distance = 0.1f;
    public Transform mirrorParent;
    public bool checkRotation;
    public Vector3 focus = Vector3.up * 10f;

    private Vector3 originalPos;

    // Use this for initialization
    private void Start()
    {
        originalPos = transform.localPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        var direction = (focus - transform.position).normalized;
        direction.z = originalPos.z;
        direction.x = mirrorParent ? mirrorParent.localScale.x * direction.x : direction.x;

        if (checkRotation)
        {
            var angle = transform.parent.rotation.eulerAngles.z;
            var aMod = Mathf.Sign(transform.parent.lossyScale.x);
            direction = Quaternion.Euler(new Vector3(0, 0, -angle * aMod)) * direction;
        }

        transform.localPosition =
            Vector3.MoveTowards(transform.localPosition, originalPos + direction * distance, 0.1f);
    }
}