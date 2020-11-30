using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoiseScaler : MonoBehaviour
{
    public List<Transform> objects;
    public bool mirrored = true;
    public float xAmount = 1f;
    public float yAmount = 1f;
    public float scaleAmount = 1f;

    private Vector3[] sizes;
    private float offset;

    // Start is called before the first frame update
    void Awake()
    {
        sizes = new Vector3[objects.Count];

        var i = 0;
        objects.ForEach(o =>
        {
            if(mirrored)
                o.localScale = new Vector3(Random.value < 0.5f ? -1 : 1, Random.value < 0.5f ? -1 : 1, 1);
            sizes[i] = o.localScale;
            i++;
        });
    }

    // Update is called once per frame
    void Update()
    {
        var i = 0;
        objects.ForEach(o =>
        {
            var position = o.position;
            var noise = Mathf.PerlinNoise(position.x + offset, position.y);
            o.transform.localScale = sizes[i] * (1f - 0.4f * Mathf.Abs(noise) * scaleAmount);
            i++;
        });

        offset += Time.deltaTime;
    }
}
