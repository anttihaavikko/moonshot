using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public TMPro.TMP_Text text;
    public Image fill;
    public Color hoverFill, hoverText;
    public ButtonMenu menu;

    private Color fillColor, textColor;

    private void Awake()
    {
        fillColor = fill.color;
        textColor = text.color;
    }

    public void Focus()
    {
        Tweener.Instance.ScaleTo(transform, Vector3.one * 1.2f, 0.2f, 0, TweenEasings.BounceEaseOut);
        Tweener.Instance.RotateTo(transform, Quaternion.Euler(0, 0, Random.Range(-3f, 3f)), 0.2f, 0, TweenEasings.BounceEaseOut);
        fill.color = hoverFill;
        text.color = hoverText;
    }

    public void DeFocus()
    {
        Tweener.Instance.ScaleTo(transform, Vector3.one, 0.15f, 0, TweenEasings.BounceEaseOut);
        Tweener.Instance.RotateTo(transform, Quaternion.Euler(0, 0, 0), 0.15f, 0, TweenEasings.BounceEaseOut);
        fill.color = fillColor;
        text.color = textColor;
    }

    public void Trigger()
    {
        button.onClick.Invoke();
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
}
