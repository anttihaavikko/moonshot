using TMPro;
using UnityEngine;

public class TickBox : MonoBehaviour
{
    public TMP_Text text;
    public Transform mark;

    private Camera cam;
    private bool done;
    private bool willBeDone;

    private void Start()
    {
        cam = Camera.main;
    }

    public void MarkDone()
    {
        willBeDone = true;
    }

    public void Check()
    {
        if (!done)
        {
            done = true;

            gameObject.SetActive(true);

            this.StartCoroutine(() =>
            {
                var pos = cam.ScreenToWorldPoint(transform.position);
                AudioManager.Instance.PlayEffectAt(25, pos, 1f);
                AudioManager.Instance.PlayEffectAt(26, pos, 1f);
                AudioManager.Instance.PlayEffectAt(27, pos, 1f);
            }, 0.2f);

            Tweener.Instance.ScaleTo(mark, Vector3.one, 0.4f, 0f, TweenEasings.BounceEaseOut);
            Tweener.Instance.RotateTo(mark, Quaternion.Euler(0, 0, -180f), 0.4f, 0f, TweenEasings.BounceEaseOut);
        }
    }

    public void Mark()
    {
        if (!done && !willBeDone)
        {
            done = true;
            mark.transform.localScale = Vector3.one;
        }
    }
}