using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public Transform barrel, muzzle;
    public LayerMask collisionMask, hitMask;
    public EffectCamera effectCam;
    public Moon moon;
    public string targetTag;
    public Rigidbody2D body;
    public ShellManager shellManager;

    private float autoShotDelay;

    // Update is called once per frame
    void Update()
    {
        autoShotDelay -= Time.deltaTime;

        var hit = Physics2D.Raycast(barrel.position, barrel.transform.right, 100f, collisionMask);
        if (hit && hit.collider.gameObject.tag == targetTag && autoShotDelay < 0f && Random.value < 0.5f)
        {
            this.StartCoroutine(() =>
            {
                body.AddForceAtPosition(-barrel.right * 10f, barrel.transform.position, ForceMode2D.Impulse);
                var eff = EffectManager.Instance.AddEffect(0, muzzle.position);
                eff.transform.parent = barrel;
                effectCam.BaseEffect(0.2f);
                Shoot(barrel.position, barrel.transform.right);
            }, Random.Range(0f, 0.15f));

            autoShotDelay = 0.5f;
        }
    }

    void Shoot(Vector3 pos, Vector3 dir)
    {
        var hit = Physics2D.Raycast(pos, dir, 100f, hitMask);
        if (hit)
        {
            shellManager.Add(pos, Quaternion.Euler(0, 0, 90) * dir);

            EffectManager.Instance.AddEffect(0, hit.point);
            EffectManager.Instance.AddEffect(1, hit.point);

            AudioManager.Instance.PlayEffectAt(0, pos, 1f);
            AudioManager.Instance.PlayEffectAt(1, pos, 0.7f);

            this.StartCoroutine(() =>
            {
                AudioManager.Instance.PlayEffectAt(14, hit.point, 0.898f);
                AudioManager.Instance.PlayEffectAt(21, hit.point, 0.694f);
                AudioManager.Instance.PlayEffectAt(11, hit.point, 0.588f);
                AudioManager.Instance.PlayEffectAt(2, hit.point, 0.8f);
            }, 0.07f);

            if (hit.collider.gameObject.tag == "Player" && targetTag == "Player")
            {
                moon.Hurt();
            }

            var line = moon.linePool.Get();
            line.SetPosition(0, pos);
            line.SetPosition(1, hit.point);

            this.StartCoroutine(() => moon.linePool.ReturnToPool(line), 0.2f);
        }
    }
}
