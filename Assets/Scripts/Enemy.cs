using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 3;
    public Rigidbody2D body;
    public Flasher flasher;
    public bool bleeds = true;
    public GameObject customParticles;
    public SpriteRenderer sprite;

    private EffectCamera cam;

    private void Start()
    {
        cam = Camera.main.GetComponent<EffectCamera>();
    }

    public void Hurt(Vector3 point, Vector3 dir)
    {
        hp--;

        if(body)
        {
            body.AddForceAtPosition(dir * 10f, point, ForceMode2D.Impulse);
        }

        if(bleeds)
        {
            EffectManager.Instance.AddEffect(2, point);
        }

        cam.BaseEffect(0.3f);

        if (flasher)
            flasher.Flash();

        if (hp <= 0)
            Die();
    }

    public void Die()
    {
        if(!customParticles)
        {
            gameObject.SetActive(false);
        }
        else
        {
            sprite.enabled = false;
            gameObject.layer = 10;
            customParticles.SetActive(true);
        }

        if(bleeds)
        {
            EffectManager.Instance.AddEffect(3, transform.position);
            EffectManager.Instance.AddEffect(4, transform.position);
            EffectManager.Instance.AddEffect(5, transform.position);
            EffectManager.Instance.AddEffect(6, transform.position);
        }

        cam.BaseEffect(0.5f);
    }
}
