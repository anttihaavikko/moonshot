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
    public TMPro.TMP_Text pointsText, pointsShadow;

    private bool hasStarted;
    private int points;

    // Start is called before the first frame update
    void Start()
    {
        points = SaveManager.Instance.GetPoints();

        var num = 1;
        Levels.levelData.ToList().ForEach(level =>
        {
            var info = SaveManager.Instance.GetDataFor(num - 1);
            var b = Instantiate(buttonPrefab, container);
            b.button.interactable = num <= points + 1;
            b.text.text = b.button.interactable ? num + ". " + level.name : "Earn " + (num - 1) + " to unlock";
            if (!b.button.interactable)
            {
                b.text.fontSize = 20;
                b.text.color = b.textColor = Color.gray;
                b.extraFgs.ForEach(e => e.color = Color.gray);
            }
            b.menu = menu;
            menu.buttons.Add(b);
            b.index = num - 1;
            b.onFocus += () => ScrollTo(b.GetComponent<RectTransform>());
            b.button.onClick.AddListener(() => StartLevel(b.index));
            b.bonuses[0].SetActive(info.bonusesDone[0]);
            b.bonuses[1].SetActive(info.bonusesDone[1]);
            b.bonuses[2].SetActive(info.bonusesDone[2]);
            b.strike.SetActive(info.completed);
            b.strike.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            num++;
        });

        pointsText.text = pointsShadow.text = points + "/" + Levels.levelData.Length * 4;
        var lvl = Manager.Instance.level;
        if(lvl > 0)
        {
            this.StartCoroutine(() => menu.Focus(0, Manager.Instance.level), 0.3f);
            scrollContent.anchoredPosition = new Vector2(scrollContent.anchoredPosition.x, lvl * 100f);
        }
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
