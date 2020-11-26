﻿using System.Collections;
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

    private int messagePos;
    private string message;

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
        message = msg;
        text.text = "";
        Invoke("StartShowing", 0.25f);
    }

    void StartShowing()
    {
        StartCoroutine(UpdateMessage());
    }

    IEnumerator UpdateMessage()
    {
        while(messagePos < message.Length - 1)
        {
            messagePos++;
            if (message[messagePos] == '<')
            {
                messagePos = message.IndexOf(">", messagePos, System.StringComparison.CurrentCulture) + 1;
            }
            text.text = message.Substring(0, messagePos).Replace("(", "<color=" + hiliteColorHex + ">").Replace(")", "</color>");
            var delay = message[messagePos] == ' ' ? 0.06f : 0.02f;
            yield return new WaitForSeconds(delay);
        }
    }
}
