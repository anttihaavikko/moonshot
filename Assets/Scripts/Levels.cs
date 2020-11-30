﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Levels : MonoBehaviour
{
    public static readonly LevelData[] Data =
    {
        new LevelData("Sinistrum", "Reach the goal", GetLeftHelp(), "Nice!"),
        new LevelData("Dextrum", "Reach the goal", GetRightHelp(), "Yahoo!"),
        new LevelData("Uphill", "Reach the goal", "Yea, now I have (both) of my (guns)...", "Yay!"),
        new LevelData("Get over the hump", "Reach the goal", GetAltShotHelp(), "Yay!"),
        new LevelData("Genocide", "Kill the bats", "Die you filthy animals!", "Piece of cake!", "Kidnapping"),
        new LevelData("Flappy Moon", "Reach the goal", "If (birds) can do it...", "And they call this hard..."),
        new LevelData("The floor is lava", "Survive 5 seconds", "Time for\n(pistol ballet)!",
            "Could have done longer..."),
        new LevelData("Rush Hour", "Reach the goal", "Gotta blow past em!", "Ohh yeeah!"),
        new LevelData("1-2", "Reach the goal", "It's me, (Moon)!", "No warps!"),
        new LevelData("Warehouse", "Reach the goal", "Hey hey!", "Yay yay!"),
        new LevelData("Rise and Fall", "Reach the goal", "Oh no, looks dangerous.", "No problem!"),
        new LevelData("Trapped!", "Survive...", "Quite a (sticky situation)!", "Nice nice!", "Trapped"),
        new LevelData("Lopsided", "Survive 5 seconds.", "Looks like I (los)t my (second gun) in that hassle.",
            "Still got it!"),
        new LevelData("Half-pipe", "Reach the goal", "Gotta keep moving...", "Nice nice!"),
        new LevelData("Revenge", "Beat it!", "That (meanie) has got my (second gun)!", "Finally!"),
        new LevelData("Breakthrough", "Reach the goal", "I bet I could (blast) through that wall.", "Easy pickings!"),
        new LevelData("Crouching and Hiding", "Kill the bats", "Die you filthy animals!", "Piece of cake!"),
        new LevelData("Corridor is lava", "Survive 7 seconds", "Psh, only (7) seconds.", "Could have done double..."),
        new LevelData("Final Bout", "Kill the bats", "Die you filthy animals!", "Piece of cake!"),
        new LevelData("Stick the Landing", "Reach the goal", "Gotta be careful!", "Flawless!"),
        new LevelData("Big Boss", "Time to Die!", "We'll see about that...", "Didn't doubt my (success) for a second!", "Boss", true),
        new LevelData("Rising Sun", "Die, again!", "...", "...", null, false, true),
        new LevelData("Eclipse", "The road back home", "Lets go!", "Home sweet home!", null, false, true)
    };

    public Moon moon;
    public EffectCamera cam;
    public Transform backdrop;
    public Zoomer zoomer;
    public LevelInfo levelInfo;
    public Appearer timer;
    private readonly float farZoom = 14f;

    private readonly float moveDuration = 0.7f;
    private int current;

    private List<Level> levels;

    private void Start()
    {
        Application.targetFrameRate = 60;

        var idx = 0;
        levels = GetComponentsInChildren<Level>(true).ToList();
        levels.ForEach(lvl =>
        {
            lvl.index = idx;
            idx++;
        });
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

    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.N))
                ChangeLevel();

            if (Input.GetKeyDown(KeyCode.P))
                ChangeLevel(-1);
        }
    }

    private static string GetLeftHelp()
    {
        return Application.isMobilePlatform
            ? "Touch (left side of screen) to shoot left gun."
            : "Click <sprite=0> to shoot\n(left) gun...";
    }

    private static string GetRightHelp()
    {
        return Application.isMobilePlatform
            ? "Touch (right side of screen) to shoot right gun."
            : "Click <sprite=1> to shoot\n(right) gun...";
    }

    private static string GetAltShotHelp()
    {
        return Application.isMobilePlatform
            ? "That looks kinda high..."
            : "You can (also shoot) with <sprite=2> or <sprite=3>.";
    }

    public LevelData GetInfo(int index)
    {
        if (index >= Data.Length) return Data[Random.Range(0, Data.Length)];

        return Data[index];
    }

    public void ChangeLevel(int dir = 1)
    {
        if (moon.IsDead()) return;

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

    private void MoveCamTo(Vector3 pos, Func<float, float> ease)
    {
        Tweener.Instance.MoveTo(cam.transform, pos, moveDuration, 0f, ease);
    }

    private void Reload()
    {
        Manager.Instance.level = current;
        Manager.Instance.startPos = cam.transform.position;
        SceneChanger.Instance.StartLevel();
    }

    public Level GetCurrentLevel()
    {
        return levels[current];
    }
}

public readonly struct LevelData
{
    public readonly string name;
    public readonly string description;
    public readonly string message;
    public readonly string winMessage;
    public readonly string demo;
    public readonly bool boss;
    public readonly bool hidden;

    public LevelData(string name, string description, string message, string winMessage, string demo = null,
        bool boss = false, bool hidden = false)
    {
        this.name = name;
        this.description = description;
        this.message = message;
        this.winMessage = winMessage;
        this.demo = demo;
        this.boss = boss;
        this.hidden = hidden;
    }
}