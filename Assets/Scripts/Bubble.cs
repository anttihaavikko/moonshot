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

    private void Start()
    {
        hiliteColorHex = "#" + ColorUtility.ToHtmlStringRGB(hiliteColor);
        wrapper.SetActive(false);
        wrapper.transform.localScale = Vector3.zero;
    }

    public void Show(string message)
    {
        SetText(message);
        SetMirroring(transform.position.x > levels.GetCurrentLevel().transform.position.x);
        Show();
    }

    private void SetMirroring(bool mirrored)
    {
        var dir = mirrored ? -1f : 1f;
        transform.localScale = new Vector3(dir, 1f, 1f);
        text.transform.localScale = new Vector3(dir, 1f, 1f);
    }

    public void Show()
    {
        shown = true;
        wrapper.SetActive(true);
        Tweener.Instance.ScaleTo(wrapper.transform, Vector3.one, showDuration, 0, TweenEasings.BounceEaseOut);
    }

    public void Hide()
    {
        if (!shown) return;

        shown = false;
        Tweener.Instance.ScaleTo(wrapper.transform, Vector3.zero, showDuration, 0, TweenEasings.QuadraticEaseOut);
        this.StartCoroutine(() => wrapper.SetActive(false), showDuration);
    }

    private void SetText(string message)
    {
        text.text = message.Replace("(", "<color=" + hiliteColorHex + ">").Replace(")", "</color>");
    }
}
