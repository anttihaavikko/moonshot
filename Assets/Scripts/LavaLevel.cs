using UnityEngine;

public class LavaLevel : Level
{
    public float par = 5f;

    public override void Activate()
    {
        base.Activate();
    }

    public override void AfterInfo()
    {
        base.AfterInfo();
        levels.timer.Show();
    }

    public override void CheckEnd(float time = 0)
    {
        if (time >= par)
        {
            Complete();
            this.StartCoroutine(levels.timer.Hide, 1.5f);
        }
    }
}