using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSelect : MonoBehaviour
{
    public CustomButton buttonPrefab;
    public Transform container;
    public RectTransform scroller, scrollContent;
    public ButtonMenu menu;

    private bool hasStarted;

    // Start is called before the first frame update
    void Start()
    {
        var num = 1;
        Levels.levelData.ToList().ForEach(level =>
        {
            var b = Instantiate(buttonPrefab, container);
            b.text.text = num + ". " + level.name;
            b.menu = menu;
            menu.buttons.Add(b);
            b.index = num - 1;
            b.onFocus += () => ScrollTo(b.GetComponent<RectTransform>());
            b.button.onClick.AddListener(() => StartLevel(b.index));
            num++;
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !hasStarted)
        {
            hasStarted = true;
            SceneChanger.Instance.ChangeScene("Start");
        }
    }

    void ScrollTo(RectTransform rt)
    {
        Canvas.ForceUpdateCanvases();
        var pos = scroller.transform.InverseTransformPoint(scrollContent.position).y - scroller.transform.InverseTransformPoint(rt.position).y;
        var diff = pos - scrollContent.anchoredPosition.y;
        if(diff < 0 || diff > 500)
        {
            var dir = new Vector2(scrollContent.anchoredPosition.x, pos);
            scrollContent.anchoredPosition = Vector2.MoveTowards(scrollContent.anchoredPosition, dir, 100f);
        }
            
    }

    void StartLevel(int index)
    {
        if (hasStarted) return;

        hasStarted = true;
        Manager.Instance.level = index;
        Manager.Instance.showInfo = true;
        SceneChanger.Instance.ChangeScene("Main");
    }
}
