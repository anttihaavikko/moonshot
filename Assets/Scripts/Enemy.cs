using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 3;
    public Rigidbody2D body;

    private EffectCamera cam;

    private void Start()
    {
        cam = Camera.main.GetComponent<EffectCamera>();
    }

    public void Hurt(Vector3 point, Vector3 dir)
    {
        hp--;

        body.AddForceAtPosition(dir * 10f, point, ForceMode2D.Impulse);

        EffectManager.Instance.AddEffect(2, point);

        cam.BaseEffect(0.3f);

        if (hp <= 0)
            Die();
    }

    public void Die()
    {
        gameObject.SetActive(false);

        EffectManager.Instance.AddEffect(3, transform.position);
        EffectManager.Instance.AddEffect(4, transform.position);
        EffectManager.Instance.AddEffect(5, transform.position);

        cam.BaseEffect(0.5f);
    }
}
