using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public List<Appearer> appearers;
    public float delay = 3f;

    public TMPro.TMP_Text nameText, descText;

    public void Show(string levelName, string description)
    {
        nameText.text = levelName;
        descText.text = description;
        appearers.ForEach(a => a.ShowAfterDelay());
        Invoke("Hide", delay);
    }

    public void Hide()
    {
        appearers.ForEach(a => a.Hide());
    }
}
