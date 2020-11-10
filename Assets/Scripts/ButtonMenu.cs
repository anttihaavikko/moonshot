using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMenu : MonoBehaviour
{
    public List<CustomButton> buttons;
    public Appearer appearer;

    private int active;
    private bool state;

    public void Toggle(bool focusFirst = false)
    {
        state = !state;

        if(state)
        {
            appearer.Show();

            if(focusFirst)
            {
                Focus(0, 0);
            }
        }
        else
        {
            appearer.Hide();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        if (active >= buttons.Count) active = 0;
        if (active < 0) active = buttons.Count - 1;

        Focus(prev, active);
    }

    void Focus(int prev, int next)
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
