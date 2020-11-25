using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSoundSource : MonoBehaviour
{
    public MaterialType type;
    public float treshold = 3f;

    private bool canDo = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canDo) return;

        if(collision.relativeVelocity.magnitude > treshold)
        {
            var vol = Mathf.Clamp(collision.relativeVelocity.magnitude * 0.1f, 0f, 1f);

            switch (type)
            {
                case MaterialType.Default:
                    AudioManager.Instance.PlayEffectAt(3, transform.position, 0.392f * vol);
                    AudioManager.Instance.PlayEffectAt(1, transform.position, 0.514f * vol);
                    break;
                case MaterialType.Wood:
                    AudioManager.Instance.PlayEffectAt(10, transform.position, 0.75f * vol);
                    AudioManager.Instance.PlayEffectAt(3, transform.position, 0.392f * vol);
                    break;
                case MaterialType.Metal:
                    AudioManager.Instance.PlayEffectAt(17, transform.position, 1f * vol);
                    AudioManager.Instance.PlayEffectAt(14, transform.position, 1f * vol);

                    break;
            }

            canDo = false;
            Invoke("EnableBack", 0.1f);
        }
    }

    void EnableBack()
    {
        canDo = true;
    }
}

public enum MaterialType
{
    Default,
    Metal,
    Wood
}
