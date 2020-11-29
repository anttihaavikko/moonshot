using TMPro;
using UnityEngine;

public class Appearer : MonoBehaviour
{
    public bool autoShow = true;
    public float appearAfter = -1f;
    public float hideDelay;
    public bool hiddenOnWeb;
    public bool isNotUi;
    public float volume = 0.6f;
    public Vector3 hiddenSize;
    public float showDuration = 0.3f;
    public float hideDuration = 0.3f;

    public TMP_Text text;
    private Vector3 size;

    // Start is called before the first frame update
    private void Start()
    {
        size = transform.localScale;
        transform.localScale = hiddenSize;

        if (autoShow && appearAfter >= 0 && (!hiddenOnWeb || Application.platform != RuntimePlatform.WebGLPlayer))
            Invoke("Show", appearAfter);
    }

    private Vector3 SoundPos()
    {
        return isNotUi ? transform.position : GetPosFromUi();
    }

    private Vector3 GetPosFromUi()
    {
        return Camera.main.ScreenToWorldPoint(transform.position);
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
        var p = SoundPos();
        var vol = 0.7f;
        AudioManager.Instance.PlayEffectAt(22, p, 1.322f * vol);
        AudioManager.Instance.PlayEffectAt(21, p, 0.58f * vol);

        // Debug.Log("Showing " + name);
        gameObject.SetActive(true);
        Tweener.Instance.ScaleTo(transform, size, showDuration, 0f, TweenEasings.BounceEaseOut);

        if (autoHide)
            HideWithDelay();
    }

    public void Hide()
    {
        CancelInvoke("Show");

        // Debug.Log("Hiding " + name);

        var p = SoundPos();
        var vol = 0.5f;
        AudioManager.Instance.PlayEffectAt(22, p, 1.322f * vol);
        AudioManager.Instance.PlayEffectAt(21, p, 0.58f * vol);

        Tweener.Instance.ScaleTo(transform, hiddenSize, hideDuration, 0f, TweenEasings.QuadraticEaseInOut);

        Invoke("AfterHide", hideDuration);
    }

    private void AfterHide()
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