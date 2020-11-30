using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSounder : MonoBehaviour
{
    public void PullGun()
    {
        var position = transform.position;
        const float vol = 0.75f;
        AudioManager.Instance.PlayEffectAt(6, position, 0.163f * vol);
        AudioManager.Instance.PlayEffectAt(7, position, 0.547f * vol);
        AudioManager.Instance.PlayEffectAt(8, position, 1f * vol);
        AudioManager.Instance.PlayEffectAt(14, position, 0.735f * vol);
    }

    public void Land()
    {
        var position = transform.position;
        const float vol = 0.75f;
        AudioManager.Instance.PlayEffectAt(3, position, 0.392f * vol);
        AudioManager.Instance.PlayEffectAt(1, position, 0.514f * vol);
    }

    public void Jump()
    {
        var position = transform.position;
        const float vol = 0.75f;
        AudioManager.Instance.PlayEffectAt(11, position, 0.465f * vol);
        AudioManager.Instance.PlayEffectAt(12, position, 1f * vol);
        AudioManager.Instance.PlayEffectAt(16, position, 0.302f * vol);
    }
}
