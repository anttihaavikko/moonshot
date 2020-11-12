using UnityEngine;
using UnityEngine.EventSystems;

public class DisableOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Moon moon;
    public StartView startView;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(moon)
            moon.SetClicksDisabled(true);

        if (startView)
            startView.SetClicksDisabled(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(moon)
            moon.SetClicksDisabled(false);

        if (startView)
            startView.SetClicksDisabled(false);
    }
}
