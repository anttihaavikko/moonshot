using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public TMPro.TMP_Text text;
    public float showDuration = 0.3f;
    public GameObject wrapper;
    public Color hiliteColor;
    public Levels levels;

    private bool shown;
    private string hiliteColorHex;
    private bool locked;

    private void Start()
    {
        hiliteColorHex = "#" + ColorUtility.ToHtmlStringRGB(hiliteColor);
        wrapper.SetActive(false);
        wrapper.transform.localScale = Vector3.zero;
    }

    public void Show(string message, bool permanent = false)
    {
        SetText(message);
        SetMirroring(transform.position.x > levels.GetCurrentLevel().transform.position.x);
        Show(permanent);
    }

    private void SetMirroring(bool mirrored)
    {
        var dir = mirrored ? -1f : 1f;
        transform.localScale = new Vector3(dir, 1f, 1f);
        text.transform.localScale = new Vector3(dir, 1f, 1f);
    }

    private Vector3 GetSize()
	{
		return Vector3.one * (Application.isMobilePlatform ? 1.5f : 1.2f);
	}

    public void Show(bool permanent = false)
    {
        locked = permanent;
        shown = true;
        wrapper.SetActive(true);
        Tweener.Instance.ScaleTo(wrapper.transform, GetSize(), showDuration, 0, TweenEasings.BounceEaseOut);
    }

    public void Hide(bool forced = false)
    {
        if (!shown || (locked && !forced)) return;

        shown = false;
        Tweener.Instance.ScaleTo(wrapper.transform, Vector3.zero, showDuration, 0, TweenEasings.QuadraticEaseOut);
        this.StartCoroutine(() => wrapper.SetActive(false), showDuration);
    }

    private void SetText(string message)
    {
        text.text = message.Replace("(", "<color=" + hiliteColorHex + ">").Replace(")", "</color>");
    }
}
