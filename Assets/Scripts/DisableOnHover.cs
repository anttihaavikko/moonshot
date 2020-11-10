using UnityEngine;
using UnityEngine.EventSystems;

public class DisableOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Moon moon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        moon.SetClicksDisabled(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        moon.SetClicksDisabled(false);
    }
}
