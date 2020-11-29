using Anima2D;
using UnityEngine;

[ExecuteInEditMode]
public class BoneToLine : MonoBehaviour
{
    public bool bezier;
    public Bone2D[] bones;
    public float lineZ;
    private LineRenderer line;

    // Use this for initialization
    private void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        line.SetPosition(0, new Vector3(transform.position.x, transform.position.y, lineZ));

        if (bezier)
            for (var i = 1; i < line.positionCount; i++)
            {
                // B(t) = (1-t)^2P0 + 2(1-t)tP1 + t2P2 , 0 < t < 1

                var t = i / (float) line.positionCount;
                var p = Mathf.Pow(1 - t, 2) * transform.position + 2 * (1 - t) * t * bones[0].endPosition +
                        Mathf.Pow(t, 2) * bones[1].endPosition;
                line.SetPosition(i, p);
            }
        else
            for (var i = 0; i < bones.Length; i++)
                line.SetPosition(i + 1, new Vector3(bones[i].endPosition.x, bones[i].endPosition.y, lineZ));
    }
}