using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : SingletonManager<SaveManager>
{
    private SaveData data;

    // Start is called before the first frame update
    private void Start()
    {
        if (PlayerPrefs.HasKey("MoonSave"))
        {
            var json = PlayerPrefs.GetString("MoonSave");
            data = JsonUtility.FromJson<SaveData>(json);
            return;
        }
        
        data = new SaveData();
    }

    public void CompleteLevel(int level)
    {
        data.levelSaveData[level].completed = true;
        Save();
    }

    public bool CompleteBonus(int level, int bonus)
    {
        var wasDone = data.levelSaveData[level].bonusesDone[bonus];
        data.levelSaveData[level].bonusesDone[bonus] = true;
        Save();
        return wasDone;
    }

    private void Save()
    {
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("MoonSave", json);
        print("Saved");
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

[Serializable]
public class SaveData
{
    public List<LevelSaveData> levelSaveData;
    public List<string> seenDemos;

    public SaveData()
    {
        levelSaveData = Levels.Data.Select(lvl => new LevelSaveData()).ToList();
        seenDemos = new List<string>();
    }
}

[Serializable]
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