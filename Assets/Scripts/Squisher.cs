using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squisher : MonoBehaviour
{
    public Moon moon;
    public Enemy enemy;
    public List<Transform> spots;
    public List<float> radii;
    public LayerMask mask;

    private float safeDuration;

    private void Update()
    {
        if(safeDuration > 0f)
        {
            safeDuration -= Time.deltaTime;
            return;
        }

        for(var i = 0; i < spots.Count; i++)
        {
            var hit = Physics2D.OverlapCircle(spots[i].position, radii[i], mask);
            if(hit)
            {
                if (moon && !moon.HasDied())
                {
                    //print("Hurt for " + spots[i].name + " and " + hit.gameObject.name);
                    if(moon.Hurt())
                    {
                        enabled = false;
                    }
                    else
                    {
                        safeDuration = 0.15f;
                    }
                }

                if (enemy)
                {
                    enemy.Die();
                    enabled = false;
                }

                return;
            }
        }
    }
}