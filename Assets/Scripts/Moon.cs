using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Moon : MonoBehaviour, IDier
{
    public Rigidbody2D leftHand, rightHand;
    public Transform leftBarrel, rightBarrel;
    public Transform leftMuzzle, rightMuzzle;
    public GameObject leftGun, rightGun;
    public Rigidbody2D body;
    public float amount = 1f;
    public EffectCamera effectCam;
    public LineRenderer linePrefab;
    public LinePool linePool;
    public TMPro.TMP_Text timerText, bestText, timerShadow, bestShadow;
    public LayerMask collisionMask;
    public Bubble bubble;
    public LevelInfo levelInfo;
    public List<Joint2D> joints, extraJoints;
    public SpriteRenderer sprite;
    public List<GameObject> visibleObjects;
    public Flasher flasher;

    private Level level;

    private Camera cam;
    private float touchTimer, bestTime = 0f;
    private bool hasTouched;
    private bool hasDied;

    private float autoShotDelay;
    private bool autoShoot = false;

    private int hp = 3;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckShots();

        if (Input.GetKeyDown(KeyCode.R))
        {
            //SceneManager.LoadSceneAsync("Main");
            Die();
        }

        CheckTouch();
    }

    bool LeftTouch()
    {
        var newTouches = Input.touches.Where(t => t.phase == TouchPhase.Began);
        return newTouches.Any(t => t.position.x < Screen.width / 2f) && Application.isMobilePlatform;
    }

    bool RightTouch()
    {
        var newTouches = Input.touches.Where(t => t.phase == TouchPhase.Began);
        return newTouches.Any(t => t.position.x > Screen.width / 2f) && Application.isMobilePlatform;
    }

    bool LeftMouse()
    {
        return Input.GetMouseButtonDown(0) && !Application.isMobilePlatform;
    }

    bool RightMouse()
    {
        return Input.GetMouseButtonDown(1) && !Application.isMobilePlatform;
    }

    void CheckShots()
    {
        if (levelInfo.IsShown() || hasDied) return;

        if (LeftTouch() || LeftMouse() || Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (leftGun.activeSelf)
            {
                ShootLeft();
            }
        }

        if (RightTouch() || RightMouse() || Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (rightGun.activeSelf)
            {
                ShootRight();
            }
        }

        if(autoShoot)
        {
            autoShotDelay -= Time.deltaTime;

            var hit = Physics2D.Raycast(rightBarrel.position, rightBarrel.transform.right, 100f, collisionMask);
            if (hit && hit.collider.gameObject.tag == "Enemy" && autoShotDelay < 0f && Random.value < 0.1f)
            {
                ShootRight();
                autoShotDelay = 0.5f;
            }

            hit = Physics2D.Raycast(leftBarrel.position, -leftBarrel.transform.right, 100f, collisionMask);
            if (hit && hit.collider.gameObject.tag == "Enemy" && autoShotDelay < 0f && Random.value < 0.1f)
            {
                ShootLeft();
                autoShotDelay = 0.5f;
            }
        }
    }

    public bool IsDead()
    {
        return hasDied;
    }

    private void ShootRight()
    {
        rightHand.AddForce(-rightHand.transform.right * amount, ForceMode2D.Impulse);
        var eff = EffectManager.Instance.AddEffect(0, rightMuzzle.position);
        eff.transform.parent = rightBarrel;
        effectCam.BaseEffect(0.2f);
        Shoot(rightBarrel.position, rightBarrel.transform.right);
    }

    private void ShootLeft()
    {
        leftHand.AddForce(leftHand.transform.right * amount, ForceMode2D.Impulse);
        var eff = EffectManager.Instance.AddEffect(0, leftMuzzle.position);
        eff.transform.parent = leftBarrel;
        effectCam.BaseEffect(0.2f);
        Shoot(leftBarrel.position, -leftHand.transform.right);
    }

    public void SetLevel(Level l)
    {
        level = l;
    }

    void CheckTouch()
    {
        if (touchTimer > bestTime)
            bestTime = touchTimer;

        timerText.text = timerShadow.text = touchTimer.ToString("F1");
        bestText.text = bestShadow.text = "BEST  " + bestTime.ToString("F1");

        if(!hasTouched)
            touchTimer += Time.deltaTime;
    }

    void Shoot(Vector3 pos, Vector3 dir)
    {
        hasTouched = false;

        bubble.Hide();

        var hit = Physics2D.Raycast(pos, dir, 100f, collisionMask);
        if(hit)
        {
            EffectManager.Instance.AddEffect(0, hit.point);
            EffectManager.Instance.AddEffect(1, hit.point);

            if(hit.collider.gameObject.tag == "Enemy" || hit.collider.gameObject.tag == "Breakable")
            {
                var e = hit.collider.GetComponent<Enemy>();
                if(e)
                {
                    e.Hurt(hit.point, dir.normalized);
                    level.CheckEnd(touchTimer);
                }
            }

            var line = linePool.Get();
            line.SetPosition(0, pos);
            line.SetPosition(1, hit.point);

            this.StartCoroutine(() => linePool.ReturnToPool(line), 0.1f);
        }
    }

    public void Touched()
    {
        level.CheckEnd(touchTimer);
        touchTimer = 0f;
        hasTouched = true;
    }

    public void SetGuns(bool hasLeftGun, bool hasRightGun)
    {
        leftGun.SetActive(hasLeftGun);
        rightGun.SetActive(hasRightGun);
    }

    public bool HasDied()
    {
        return hasDied;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDied) return;

        if(collision.gameObject.tag == "Flag" && level)
        {
            level.Complete();
        }

        if (collision.gameObject.tag == "Bubble")
        {
            var trigger = collision.GetComponent<BubbleTrigger>();
            var msg = trigger.GetMessage();
            if (!trigger.shown && !Manager.Instance.IsShown(msg))
            {
                this.StartCoroutine(() => {
                    if(!hasDied)
                    {
                        bubble.Show(msg);
                    }
                }, trigger.delay);
                Manager.Instance.Add(msg);
                trigger.shown = true;
            }
        }
    }

    public void Die()
    {
        if (hasDied) return;

        hasDied = true;
        bubble.Hide();

        level.Restart();

        visibleObjects.ForEach(vo => vo.SetActive(false));
        joints.ForEach(ThrowBody);
        extraJoints.Where(j => Random.value > 0.3f).ToList().ForEach(ThrowBody);

        sprite.enabled = false;

        gameObject.layer = 10;
        body.gravityScale = 0;

        EffectManager.Instance.AddEffect(3, transform.position);
        EffectManager.Instance.AddEffect(4, transform.position);
        EffectManager.Instance.AddEffect(5, transform.position);
        EffectManager.Instance.AddEffect(6, transform.position);
    }

    void ThrowBody(Joint2D joint)
    {
        joint.enabled = false;
        var dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
        joint.attachedRigidbody.AddForce(dir * 15f, ForceMode2D.Impulse);
    }

    public bool Hurt()
    {
        flasher.Flash();

        hp--;

        if (hp <= 0)
        {
            Die();
            return true;
        }

        effectCam.BaseEffect(0.1f);

        return false;
    }
}
