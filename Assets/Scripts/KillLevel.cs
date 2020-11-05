using System.Collections.Generic;
using System.Linq;

public class KillLevel : Level
{
    public List<Enemy> targets;

    public override void Activate()
    {
        base.Activate();
    }

    public override void CheckEnd(float time = 0)
    {
        if (targets.All(e => e.hp <= 0))
        {
            Complete();
        }
    }
}