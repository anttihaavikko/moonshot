using UnityEngine;

public class Enemy : MonoBehaviour, IDier
{
    public int hp = 3;
    public Rigidbody2D body;
    public Flasher flasher;
    public bool bleeds = true;
    public GameObject customParticles;
    public SpriteRenderer sprite;
    public float moveForce = 1f;
    public Transform spawnOnDeath;

    private EffectCamera cam;
    private bool hasDied;

    private void Start()
    {
        cam = Camera.main.GetComponent<EffectCamera>();
    }

    public void Die()
    {
        if (hasDied) return;

        if (spawnOnDeath)
        {
            EffectManager.Instance.AddEffect(3, transform.position);
            EffectManager.Instance.AddEffect(1, transform.position);
            spawnOnDeath.position = transform.position;
        }

        if (bleeds)
        {
            EffectManager.Instance.AddEffect(3, transform.position);
            EffectManager.Instance.AddEffect(4, transform.position);
            EffectManager.Instance.AddEffect(5, transform.position);
            EffectManager.Instance.AddEffect(6, transform.position);
        }

        this.StartCoroutine(() =>
        {
            AudioManager.Instance.PlayEffectAt(1, transform.position, 0.669f);
            AudioManager.Instance.PlayEffectAt(6, transform.position, 1.233f);
            AudioManager.Instance.PlayEffectAt(5, transform.position, 1.005f);
            AudioManager.Instance.PlayEffectAt(4, transform.position, 1.204f);
            AudioManager.Instance.PlayEffectAt(13, transform.position, 1.192f);

            if (!customParticles)
            {
                gameObject.SetActive(false);
            }
            else
            {
                sprite.enabled = false;
                gameObject.layer = 10;
                customParticles.SetActive(true);
            }
        }, 0.07f);

        cam.BaseEffect(0.5f);
    }

    public void Hurt(Vector3 point, Vector3 dir)
    {
        if (body) body.AddForceAtPosition(dir * 10f * moveForce, point, ForceMode2D.Impulse);

        if (bleeds)
        {
            EffectManager.Instance.AddEffect(2, point);

            if (hp > 0)
                this.StartCoroutine(() =>
                {
                    AudioManager.Instance.PlayEffectAt(8, transform.position, 1f);
                    AudioManager.Instance.PlayEffectAt(12, transform.position, 0.767f);
                    AudioManager.Instance.PlayEffectAt(14, transform.position, 1.184f);
                    AudioManager.Instance.PlayEffectAt(16, transform.position, 0.384f);
                    AudioManager.Instance.PlayEffectAt(11, transform.position, 0.588f);
                    AudioManager.Instance.PlayEffectAt(15, transform.position, 0.457f);
                }, 0.1f);
        }

        Damage(1);
    }

    public void Damage(int amount)
    {
        hp -= amount;

        cam.BaseEffect(0.3f);

        if (flasher)
            flasher.Flash();

        if (hp <= 0)
            Die();
    }
}

public interface IDier
{
    void Die();
}