using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bubble : MonoBehaviour
{
    public TMPro.TMP_Text text;
    public float showDuration = 0.3f;
    public GameObject wrapper;
    public Color hiliteColor;
    public Levels levels;
    public UnityAction afterHide;

    private bool shown;
    private string hiliteColorHex;
    private bool locked;

    private int messagePos;
    private string message;
    private bool endReached;

    private void Start()
    {
        hiliteColorHex = "#" + ColorUtility.ToHtmlStringRGB(hiliteColor);
        wrapper.SetActive(false);
        wrapper.transform.localScale = Vector3.zero;
    }

    public void ShowWithMirroring(string message, bool mirrored)
    {
        SetText(message);
        SetMirroring(mirrored);
        Show(false);
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

    private void SetText(string msg)
    {
        messagePos = 0;
        endReached = false;
        message = msg;
        text.text = "";
        Invoke("StartShowing", 0.25f);
    }

    void StartShowing()
    {
        StartCoroutine(UpdateMessage());
    }

    private void Update()
    {
        if(Input.anyKeyDown)
        {
            SkipOrHide();
        }
    }

    public void SkipOrHide()
    {
        if(shown)
        {
            if(!endReached)
            {
                messagePos = message.Length - 2;
                endReached = true;
            }
            else
            {
                Hide();
                afterHide?.Invoke();
            }
        }
    }

    IEnumerator UpdateMessage()
    {
        var first = true;
        while(messagePos < message.Length)
        {
            if(first)
            {
                AudioManager.Instance.PlayEffectAt(Random.Range(29, 41), transform.position, 2f);
            }
            if (message[messagePos] == '<')
            {
                messagePos = message.IndexOf(">", messagePos, System.StringComparison.CurrentCulture) + 1;
            }
            else
            {
                messagePos++;
            }
            text.text = message.Substring(0, messagePos).Replace("(", "<color=" + hiliteColorHex + ">").Replace(")", "</color>");
            first = message[messagePos - 1] == ' ';
            var delay = first ? 0.07f : 0.03f;
            yield return new WaitForSeconds(delay);
        }

        endReached = true;
    }
}
