﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Appearer : MonoBehaviour
{
    public bool autoShow = true;
	public float appearAfter = -1f;
	public float hideDelay;
    public bool silent;
    public bool hiddenOnWeb;
    public bool soundOnZero;
    public float volume = 0.6f;
    public Vector3 hiddenSize;
    public float showDuration = 0.3f;
    public float hideDuration = 0.3f;

    public TMP_Text text;
    private Vector3 size;

    // Start is called before the first frame update
    void Start()
    {
        size = transform.localScale;
        transform.localScale = hiddenSize;

		if (autoShow && appearAfter >= 0 && (!hiddenOnWeb || Application.platform != RuntimePlatform.WebGLPlayer))
			Invoke("Show", appearAfter);
    }

    private Vector3 SoundPos()
    {
        return soundOnZero ? Vector3.zero : transform.position;
    }

    public void Show()
    {
        Show(false);
    }

    public void ShowAfterDelay()
    {
        Invoke("Show", Mathf.Max(appearAfter, 0f));
    }

    public void Show(bool autoHide)
    {
        if(!silent)
        {
            var p = SoundPos();
            //AudioManager.Instance.PlayEffectAt(1, transform.position, 1f * volume);
            //AudioManager.Instance.PlayEffectAt(4, transform.position, 0.669f * volume);
            //AudioManager.Instance.PlayEffectAt(9, transform.position, 0.506f * volume);
            //AudioManager.Instance.PlayEffectAt(22, transform.position, 0.735f * volume);
        }

        // Debug.Log("Showing " + name);
        Tweener.Instance.ScaleTo(transform, size, showDuration, 0f, TweenEasings.BounceEaseOut);

        if (autoHide)
            HideWithDelay();
    }

    public void Hide()
	{
        CancelInvoke("Show");

        // Debug.Log("Hiding " + name);

        if(!silent)
        {
            var p = SoundPos();
            //AudioManager.Instance.PlayEffectAt(1, transform.position, 1f * volume);
            //AudioManager.Instance.PlayEffectAt(4, transform.position, 0.669f * volume);
            //AudioManager.Instance.PlayEffectAt(9, transform.position, 0.506f * volume);
            //AudioManager.Instance.PlayEffectAt(22, transform.position, 0.735f * volume);
        }

        Tweener.Instance.ScaleTo(transform, hiddenSize, hideDuration, 0f, TweenEasings.QuadraticEaseInOut);

        Invoke("AfterHide", hideDuration);
	}

    void AfterHide()
    {
        gameObject.SetActive(false);
    }

    public void HideWithDelay()
	{
		Invoke("Hide", hideDelay);
	}

    public void ShowWithText(string t, float delay)
    {
        if (text)
            text.text = t;

        Invoke("Show", delay);
    }
}
