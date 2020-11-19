using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMenu : MonoBehaviour
{
    public List<CustomButton> buttons;
    public Appearer appearer;
    public bool startVisible;
    public bool loops = true;
    public bool togglable = true;
    public LevelInfo levelInfo;

    private int active;
    private bool state;

    private void Start()
    {
        if(startVisible)
        {
            this.StartCoroutine(() => Toggle(true), 0.2f);
        }
    }

    public void ShowEnd()
    {
        Toggle(false);
        Focus(0, 1);
    }

    public void Toggle(bool focusFirst = false)
    {
        state = !state;

        if(state)
        {
            appearer.Show();

            if (levelInfo)
            {
                levelInfo.closeWithAny = false;
                levelInfo.Show();
            }

            if (focusFirst)
            {
                Focus(0, 0);
            }
        }
        else
        {
            appearer.Hide();

            if (levelInfo)
                levelInfo.Hide();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (togglable && Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle(true);
        }

        if (!state) return;

        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MoveFocus(-1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveFocus(1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            buttons[active].Trigger();
        }
    }

    void MoveFocus(int direction)
    {
        var prev = active;

        active += direction;
        if (active >= buttons.Count) active = loops ? 0 : buttons.Count - 1;
        if (active < 0) active = loops ? buttons.Count - 1 : 0;

        Focus(prev, active);
    }

    public void Focus(int prev, int next)
    {
        active = next;
        buttons[prev].DeFocus();
        buttons[next].Focus();
    }

    public void Focus(CustomButton button)
    {
        Focus(active, buttons.IndexOf(button));
    }
}
