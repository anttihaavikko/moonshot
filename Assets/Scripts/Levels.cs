using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Levels : MonoBehaviour
{
    public Moon moon;
    public EffectCamera cam;
    public Transform backdrop;
    public Zoomer zoomer;

    private List<Level> levels;
    private int current;

    private readonly float moveDuration = 0.7f;
    private readonly float farZoom = 14f;

    private void Start()
    {
        levels = GetComponentsInChildren<Level>(true).ToList();

        if(Manager.Instance.level > -1)
        {
            levels.ForEach(l => l.gameObject.SetActive(false));
            zoomer.ZoomTo(farZoom, true);
            current = Manager.Instance.level;
            cam.transform.position = Manager.Instance.startPos;
            cam.enabled = false;
            MoveCamTo(levels[current].GetCamPos(), TweenEasings.QuadraticEaseInOut);
            this.StartCoroutine(() =>
            {
                zoomer.ZoomTo(8.5f);
                Manager.Instance.startPos = cam.transform.position;
                cam.enabled = true;
                cam.ResetOrigin();
            }, moveDuration);
        }
        else
        {
            var act = levels.First(l => l.gameObject.activeSelf);
            if (act)
            {
                current = levels.IndexOf(act);
                cam.transform.position = levels[current].GetCamPos();
                cam.ResetOrigin();
            }
        }

        levels[current].Activate();
    }

    private void Update()
    {
        if(Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.N))
                ChangeLevel(1);

            if (Input.GetKeyDown(KeyCode.P))
                ChangeLevel(-1);
        }
    }

    public void ChangeLevel(int dir = 1)
    {
        zoomer.ZoomTo(farZoom);

        this.StartCoroutine(() =>
        {
            cam.enabled = false;
            current += dir;

            if (current >= levels.Count) current = 0;
            if (current < 0) current = levels.Count - 1;

            var next = levels[current];
            MoveCamTo((next.transform.position + cam.transform.position) * 0.5f, TweenEasings.QuadraticEaseInOut);
            Invoke("Reload", moveDuration);
        }, 0.5f);
    }

    void MoveCamTo(Vector3 pos, System.Func<float, float> ease)
    {
        Tweener.Instance.MoveTo(cam.transform, pos, moveDuration, 0f, ease);
    }

    void Reload()
    {
        Manager.Instance.level = current;
        Manager.Instance.startPos = cam.transform.position;
        SceneManager.LoadSceneAsync("Main");
    }   
}
