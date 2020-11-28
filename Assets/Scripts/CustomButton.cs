using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public TMPro.TMP_Text text;
    public Image fill;
    public List<Image> extraFgs, extraBgs;
    public Color hoverFill, hoverText;
    public ButtonMenu menu;
    public UnityAction onFocus;
    public int index;
    public List<GameObject> bonuses;
    public GameObject strike;
    public Color fillColor, textColor;
    public bool isNextLevelButton;

    private Camera cam;

    private void Awake()
    {
        fillColor = fill.color;
        textColor = text.color;
        cam = Camera.main;

        button.onClick.AddListener(ClickSound);
    }

    public void Focus()
    {
        Tweener.Instance.ScaleTo(transform, Vector3.one * 1.2f, 0.2f, 0, TweenEasings.BounceEaseOut);
        Tweener.Instance.RotateTo(transform, Quaternion.Euler(0, 0, Random.Range(-3f, 3f)), 0.2f, 0, TweenEasings.BounceEaseOut);

        if(button.interactable)
        {
            fill.color = hoverFill;
            extraBgs.ForEach(e => e.color = hoverFill);
            extraFgs.ForEach(e => e.color = hoverText);
            text.color = hoverText;
        }

        if (onFocus != null)
            onFocus.Invoke();

        var p = cam.ScreenToWorldPoint(transform.position);
        var vol = 0.6f;
        AudioManager.Instance.PlayEffectAt(3, p, 0.31f * vol);
        AudioManager.Instance.PlayEffectAt(2, p, 0.996f * vol);
        AudioManager.Instance.PlayEffectAt(21, p, 1.347f * vol);

    }

    public void DeFocus()
    {
        Tweener.Instance.ScaleTo(transform, Vector3.one, 0.15f, 0, TweenEasings.BounceEaseOut);
        Tweener.Instance.RotateTo(transform, Quaternion.Euler(0, 0, 0), 0.15f, 0, TweenEasings.BounceEaseOut);

        if(button.interactable)
        {
            fill.color = fillColor;
            extraBgs.ForEach(e => e.color = fillColor);
            text.color = textColor;
            extraFgs.ForEach(e => e.color = textColor);
        }
    }

    public void Trigger()
    {
        if(button.interactable)
        {
            button.onClick.Invoke();
        }
    }

    public void ClickSound()
    {
        var p = cam.ScreenToWorldPoint(transform.position);
        AudioManager.Instance.PlayEffectAt(26, p, 1f);
        AudioManager.Instance.PlayEffectAt(25, p, 1f);
        AudioManager.Instance.PlayEffectAt(24, p, 1.257f);
        AudioManager.Instance.PlayEffectAt(1, p, 1f);
    }

    public void ChangeScene(string scene)
    {
        SceneChanger.Instance.ChangeScene(scene);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(menu)
        {
            menu.Focus(this);
        }
        else
        {
            Focus();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeFocus();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void DisableOrEnable()
    {
        if(isNextLevelButton)
        {
            button.interactable = SaveManager.Instance.GetPoints() > Manager.Instance.level;
            text.color = button.interactable ? Color.white : Color.gray;
        }
    } 
}
