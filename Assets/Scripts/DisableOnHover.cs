using UnityEngine;
using UnityEngine.EventSystems;

public class DisableOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Moon moon;
    public StartView startView;
    public LevelInfo levelInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (moon)
            moon.SetClicksDisabled(true);

        if (startView)
            startView.SetClicksDisabled(true);

        if (levelInfo)
            levelInfo.SetClicksDisabled(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (moon)
            moon.SetClicksDisabled(false);

        if (startView)
            startView.SetClicksDisabled(false);

        if (levelInfo)
            levelInfo.SetClicksDisabled(false);
    }
}