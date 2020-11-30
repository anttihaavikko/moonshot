using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public int level = -1;
    public Vector3 startPos = Vector3.zero;
    public bool showInfo = true;

    private List<string> shownMessages;

    public static Manager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        shownMessages = new List<string>();

        DontDestroyOnLoad(gameObject);
    }

    public void Add(string message)
    {
        shownMessages.Add(message);
    }

    public bool IsShown(string message)
    {
        return shownMessages.Contains(message);
    }

    public int GetLevelLimit()
    {
        if (level == -1) return 0;
        if (Levels.Data[level + 1].hidden) return 999;
        return Levels.Data[level].boss ? Levels.BossLevelLimit : level;
    }
}