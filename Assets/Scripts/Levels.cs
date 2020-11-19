using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Levels : MonoBehaviour
{
    public Moon moon;
    public EffectCamera cam;
    public Transform backdrop;
    public Zoomer zoomer;
    public LevelInfo levelInfo;
    public Appearer timer;

    private List<Level> levels;
    private int current;

    private readonly float moveDuration = 0.7f;
    private readonly float farZoom = 14f;

    public static readonly LevelData[] levelData = {
        new LevelData("Sinistrum", "Reach the goal", GetLeftHelp(), "Nice!"), // 0
        new LevelData("Dextrum", "Reach the goal", GetRightHelp(), "Yahoo!"), // 1
        new LevelData("Get over the hump", "Reach the goal", "Okay, now I have (both) of my (guns)...", "Yay!"), // 8
        new LevelData("Genocide", "Kill the bats", "Die you filthy animals!", "Piece of cake!"), // 2
        new LevelData("Flappy Moon", "Reach the goal", "If (birds) can do it...", "And they call this hard..."), // 6
        new LevelData("The floor is lava", "Survive 5 seconds", "Time for\n(pistol ballet)!", "Could have done longer..."), // 3
        new LevelData("Rush Hour", "Reach the goal", "Gotta blow past em!", "Ohh yeeah!"), // 9
        new LevelData("1-2", "Reach the goal", "It's me, (Moon)!", "No warps!"), // 7
        new LevelData("Rise and Fall", "Reach the goal", "Oh no, looks dangerous.", "No problem!"), // 10
        new LevelData("Breakthrough", "Reach the goal", "I bet I could (blast) through that wall.", "Easy pickings!"), // 4
        new LevelData("Genocide", "Kill the bats", "Die you filthy animals!", "Piece of cake!"), // 2
        new LevelData("The floor is lava", "Survive 7 seconds", "Psh, only (7) seconds.", "Could have done double..."), // 5
        new LevelData("Genocide", "Kill the bats", "Die you filthy animals!", "Piece of cake!"), // 2
        new LevelData("Stick the Landing", "Reach the goal", "Gotta be careful!", "Flawless!"), // 11
    };

    private static string GetLeftHelp()
    {
        return Application.isMobilePlatform ? "Touch (left side of screen) to shoot left gun." : "Use (LMB) to shoot left hand gun.";
    }

    private static string GetRightHelp()
    {
        return Application.isMobilePlatform ? "Touch (right side of screen) to shoot right gun." : "Use (RMB) to shoot right hand gun.";
    }


    private void Start()
    {
        Application.targetFrameRate = 60;

        levels = GetComponentsInChildren<Level>(true).OrderBy(lvl => lvl.index).ToList();
        zoomer.ZoomTo(farZoom, true);

        if (Manager.Instance.level > -1)
        {
            levels.ForEach(l => l.gameObject.SetActive(false));
            current = Manager.Instance.level;
            cam.transform.position = Manager.Instance.startPos;
            cam.enabled = false;
            MoveCamTo(levels[current].GetCamPos(), TweenEasings.QuadraticEaseInOut);
            this.StartCoroutine(() =>
            {
                zoomer.ZoomTo(levels[current].zoom);
                Manager.Instance.startPos = cam.transform.position;
                cam.enabled = true;
                cam.ResetOrigin();
            }, moveDuration);
        }
        else
        {
            var act = levels.First(l => l.gameObject.activeSelf);
            if (act)
            {
                current = levels.IndexOf(act);
                zoomer.ZoomTo(levels[current].zoom);
                cam.transform.position = levels[current].GetCamPos();
                cam.ResetOrigin();
            }
        }

        levels[current].Activate();
    }

    public LevelData GetInfo(int index)
    {
        if(index >= levelData.Length)
        {
            return levelData[Random.Range(0, levelData.Length)];
        }

        return levelData[index];
    }

    private void Update()
    {
        if(Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.N))
                ChangeLevel(1);

            if (Input.GetKeyDown(KeyCode.P))
                ChangeLevel(-1);
        }
    }

    public void ChangeLevel(int dir = 1)
    {
        if(moon.IsDead())
        {
            return;
        }

        Manager.Instance.showInfo = true;
        zoomer.ZoomTo(farZoom);

        this.StartCoroutine(() =>
        {
            cam.enabled = false;
            current += dir;

            if (current >= levels.Count) current = 0;
            if (current < 0) current = levels.Count - 1;

            var next = levels[current];
            MoveCamTo((next.transform.position + cam.transform.position) * 0.5f, TweenEasings.QuadraticEaseInOut);
            Invoke("Reload", moveDuration);
        }, 0.5f);
    }

    public void AfterInfo()
    {
        levels[current].AfterInfo();
    }

    void MoveCamTo(Vector3 pos, System.Func<float, float> ease)
    {
        Tweener.Instance.MoveTo(cam.transform, pos, moveDuration, 0f, ease);
    }

    void Reload()
    {
        Manager.Instance.level = current;
        Manager.Instance.startPos = cam.transform.position;
        SceneManager.LoadSceneAsync("Main");
    }

    public Level GetCurrentLevel()
    {
        return levels[current];
    }
}

public struct LevelData
{
    public string name;
    public string description;
    public string message;
    public string winMessage;

    public LevelData(string name, string description, string message, string winMessage)
    {
        this.name = name;
        this.description = description;
        this.message = message;
        this.winMessage = winMessage;
    }
};