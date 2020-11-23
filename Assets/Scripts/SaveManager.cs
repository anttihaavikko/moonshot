using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : SingletonManager<SaveManager>
{
    private SaveData data;

    // Start is called before the first frame update
    void Start()
    {
        data = new SaveData();
    }

    public void CompleteLevel(int level)
    {
        data.levelSaveData[level].completed = true;
        //print(JsonUtility.ToJson(data));
    }

    public void CompleteBonus(int level, int bonus)
    {
        data.levelSaveData[level].bonusesDone[bonus] = true;
        //print(JsonUtility.ToJson(data));
    }

    public LevelSaveData GetDataFor(int level)
    {
        return data.levelSaveData[level];
    }

    public int GetPoints()
    {
        return data.levelSaveData.Where(lvl => lvl.completed).Select(lvl => 1 + lvl.bonusesDone.Count(b => b)).Sum();
    }

    public void MarkDemoSeen(string demo)
    {
        data.seenDemos.Add(demo);
    }

    public bool ShouldShowDemo(string demo)
    {
        return demo != null && !data.seenDemos.Contains(demo);
    }
}

[System.Serializable]
public class SaveData
{
    public List<LevelSaveData> levelSaveData;
    public List<string> seenDemos;

    public SaveData()
    {
        levelSaveData = Levels.levelData.Select(lvl => new LevelSaveData()).ToList();
        seenDemos = new List<string>();
    }
}

[System.Serializable]
public class LevelSaveData
{
    public bool completed;
    public List<bool> bonusesDone;

    public LevelSaveData()
    {
        bonusesDone = new List<bool>
        {
            false,
            false,
            false
        };
    }
}