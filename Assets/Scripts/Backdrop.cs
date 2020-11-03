using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backdrop : MonoBehaviour
{
    public List<GameObject> trees;
    public List<Transform> layers;

    // Start is called before the first frame update
    void Start()
    {
        trees.ForEach(tree => tree.SetActive(Random.value < 0.5f));
        layers.ForEach(layer => layer.localScale = new Vector3(Random.value < 0.5f ? 1f : -1f, 1f, 1f));
    }
}
