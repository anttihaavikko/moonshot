using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backdrop : MonoBehaviour
{
    public List<GameObject> trees;
    public List<Transform> layers;
    public List<SpriteRenderer> clouds;

    // Start is called before the first frame update
    void Start()
    {
        trees.ForEach(tree => tree.SetActive(Random.value < 0.5f));
        layers.ForEach(layer => layer.localScale = new Vector3(Random.value < 0.5f ? 1f : -1f, 1f, 1f));
        clouds.ForEach(cloud => {
            cloud.sortingOrder = Random.Range(-4, 0);
            var xmod = Random.value < 0.5f ? 1f : -1f;
            var ymod = Random.value < 0.5f ? 1f : -1f;
            var scale = Random.Range(0.5f, 1.5f);
            cloud.transform.localScale = new Vector3(
                cloud.transform.localScale.x * scale * xmod,
                cloud.transform.localScale.y * scale * ymod,
                cloud.transform.localScale.z
            );
        });
    }
}
