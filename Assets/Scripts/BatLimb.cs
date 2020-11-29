using UnityEngine;

public class BatLimb : MonoBehaviour
{
    public EffectCamera cam;
    public AutoShooter gun;
    public Moon moon;
    public Enemy boss;
    public BatLimb other;

    private bool broken;

    public void Break()
    {
        if (broken) return;

        other.MarkBroken();

        boss.Damage(10);

        EffectManager.Instance.AddEffect(3, transform.position);
        EffectManager.Instance.AddEffect(4, transform.position);
        EffectManager.Instance.AddEffect(5, transform.position);
        EffectManager.Instance.AddEffect(6, transform.position);

        this.StartCoroutine(() =>
        {
            AudioManager.Instance.PlayEffectAt(1, transform.position, 0.669f);
            AudioManager.Instance.PlayEffectAt(6, transform.position, 1.233f);
            AudioManager.Instance.PlayEffectAt(5, transform.position, 1.005f);
            AudioManager.Instance.PlayEffectAt(4, transform.position, 1.204f);
            AudioManager.Instance.PlayEffectAt(13, transform.position, 1.192f);

            gameObject.SetActive(false);
            gun.enabled = false;
        }, 0.07f);

        cam.BaseEffect(0.5f);

        moon.Heal();
    }

    public void MarkBroken()
    {
        broken = true;
    }
}