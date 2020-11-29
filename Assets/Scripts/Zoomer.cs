using UnityEngine;

public class Zoomer : MonoBehaviour
{
    private Camera cam;
    private float targetOrtho = 8.5f;

    // Start is called before the first frame update
    private void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    private void Update()
    {
        cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, targetOrtho, Time.deltaTime * 20f);
    }

    public void ZoomTo(float target, bool instant = false)
    {
        targetOrtho = target;

        if (instant && cam) cam.orthographicSize = targetOrtho;
    }
}