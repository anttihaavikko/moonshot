using UnityEngine;

public class BoxOutlinerScript : MonoBehaviour
{
    public float outlineWidth = 0.2f;
    public Transform innerObject;

    public void DoOutline()
    {
        Debug.Log("Fixing outline");

        var sprite = GetComponent<SpriteRenderer>();

        var x = 1f - outlineWidth / sprite.bounds.size.x;
        var y = 1f - outlineWidth / sprite.bounds.size.y;

        innerObject.localScale = new Vector3(x, y, 1f);
    }
}