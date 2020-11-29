using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    public TMP_Text timerText, bestText, timerShadow, bestShadow;
    public LayerMask collisionMask;
    public Bubble bubble;
    public LevelInfo levelInfo;
    public List<Joint2D> joints, extraJoints;
    public SpriteRenderer sprite;
    public List<GameObject> visibleObjects;
    public Flasher flasher;
    public HingeJoint2D attachJoint;
    public ShellManager shellManager;

    private float autoShotDelay;
    private bool clicksDisabled;
    private bool hasDied;
    private bool hasTouched;

    private int hp = 3;

    private Level level;

    private float touchTimer, bestTime;

    // Update is called once per frame
    private void Update()
    {
        CheckShots();

        if (Input.GetKeyDown(KeyCode.R)) SceneChanger.Instance.ChangeScene("Main");

        CheckTouch();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDied) return;

        if (collision.gameObject.CompareTag("Flag") && level) level.Complete();

        if (collision.gameObject.CompareTag("Pickup") && level)
        {
            var o = collision.gameObject;
            o.SetActive(false);

            var position = o.transform.position;
            EffectManager.Instance.AddEffect(3, position);
            EffectManager.Instance.AddEffect(6, position);

            AudioManager.Instance.PlayEffectAt(25, position, 1f);
            AudioManager.Instance.PlayEffectAt(26, position, 1f);
            AudioManager.Instance.PlayEffectAt(27, position, 1f);
        }

        if (!collision.gameObject.CompareTag("Bubble")) return;

        var trigger = collision.GetComponent<BubbleTrigger>();
        var msg = trigger.GetMessage();
        if (!trigger.shown && !Manager.Instance.IsShown(msg))
        {
            this.StartCoroutine(() =>
            {
                if (hasDied) return;
                bubble.Show(msg);
                trigger.appearers.ForEach(a => a.Show());
            }, trigger.delay);
            Manager.Instance.Add(msg);
            trigger.shown = true;
        }
    }

    public void Die()
    {
        if (hasDied) return;

        AudioManager.Instance.curMusic.pitch = 0.8f;

        level.CancelEnd();

        hasDied = true;
        bubble.Hide(true);

        level.Restart();

        visibleObjects.ForEach(vo => vo.SetActive(false));
        joints.ForEach(ThrowBody);
        extraJoints.Where(j => Random.value > 0.3f).ToList().ForEach(ThrowBody);

        sprite.enabled = false;

        gameObject.layer = 10;
        body.gravityScale = 0;

        var position = transform.position;
        EffectManager.Instance.AddEffect(3, position);
        EffectManager.Instance.AddEffect(4, position);
        EffectManager.Instance.AddEffect(5, position);
        EffectManager.Instance.AddEffect(6, position);

        effectCam.BaseEffect(0.5f);

        this.StartCoroutine(() =>
        {
            var pos = transform.position;
            AudioManager.Instance.PlayEffectAt(1, pos, 0.669f);
            AudioManager.Instance.PlayEffectAt(6, pos, 1.233f);
            AudioManager.Instance.PlayEffectAt(5, pos, 1.005f);
            AudioManager.Instance.PlayEffectAt(4, pos, 1.204f);
            AudioManager.Instance.PlayEffectAt(13, pos, 1.192f);
        }, 0.07f);
    }

    private bool LeftTouch()
    {
        var newTouches = Input.touches.Where(t => t.phase == TouchPhase.Began);
        return newTouches.Any(t => t.position.x < Screen.width / 2f) && Application.isMobilePlatform;
    }

    private bool RightTouch()
    {
        var newTouches = Input.touches.Where(t => t.phase == TouchPhase.Began);
        return newTouches.Any(t => t.position.x > Screen.width / 2f) && Application.isMobilePlatform;
    }

    private bool LeftMouse()
    {
        return !clicksDisabled && Input.GetMouseButtonDown(0) && !Application.isMobilePlatform;
    }

    private bool RightMouse()
    {
        return !clicksDisabled && Input.GetMouseButtonDown(1) && !Application.isMobilePlatform;
    }

    public void SetClicksDisabled(bool state)
    {
        clicksDisabled = state;
    }

    private void CheckShots()
    {
        if (levelInfo.IsShown() || hasDied) return;

        if (LeftTouch() || LeftMouse() || Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Alpha8))
            if (leftGun.activeSelf)
                ShootLeft();

        if (!RightTouch() && !RightMouse() && !Input.GetKeyDown(KeyCode.Alpha2) &&
            !Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKeyDown(KeyCode.D) &&
            !Input.GetKeyDown(KeyCode.Alpha9)) return;

        if (rightGun.activeSelf) ShootRight();

        // autoShotDelay -= Time.deltaTime;
        //
        // var hit = Physics2D.Raycast(rightBarrel.position, rightBarrel.transform.right, 100f, collisionMask);
        // if (hit && hit.collider.gameObject.CompareTag("Enemy") && autoShotDelay < 0f && Random.value < 0.1f)
        // {
        //     ShootRight();
        //     autoShotDelay = 0.5f;
        // }
        //
        // hit = Physics2D.Raycast(leftBarrel.position, -leftBarrel.transform.right, 100f, collisionMask);
        // if (hit && hit.collider.gameObject.CompareTag("Enemy") && autoShotDelay < 0f && Random.value < 0.1f)
        // {
        //     ShootLeft();
        //     autoShotDelay = 0.5f;
        // }
    }

    public bool IsDead()
    {
        return hasDied;
    }

    private void ShootRight()
    {
        level.AddShot("R");
        rightHand.AddForce(-rightHand.transform.right * amount, ForceMode2D.Impulse);
        var eff = EffectManager.Instance.AddEffect(0, rightMuzzle.position);
        eff.transform.parent = rightBarrel;
        effectCam.BaseEffect(0.2f);
        Shoot(rightBarrel.position, rightBarrel.transform.right);
    }

    private void ShootLeft()
    {
        level.AddShot("L");
        var right = leftHand.transform.right;
        leftHand.AddForce(right * amount, ForceMode2D.Impulse);
        var eff = EffectManager.Instance.AddEffect(0, leftMuzzle.position);
        eff.transform.parent = leftBarrel;
        effectCam.BaseEffect(0.2f);
        Shoot(leftBarrel.position, -right);
    }

    public void SetLevel(Level l)
    {
        level = l;
    }

    private void CheckTouch()
    {
        if (touchTimer > bestTime)
            bestTime = touchTimer;

        timerText.text = timerShadow.text = touchTimer.ToString("F1");
        bestText.text = bestShadow.text = "BEST  " + bestTime.ToString("F1");

        if (!hasTouched)
            touchTimer += Time.deltaTime;
    }

    private void Shoot(Vector3 pos, Vector3 dir)
    {
        hasTouched = false;

        var hit = Physics2D.Raycast(pos, dir, 100f, collisionMask);
        if (!hit) return;
        shellManager.Add(pos, Quaternion.Euler(0, 0, 90) * dir);

        EffectManager.Instance.AddEffect(0, hit.point);
        EffectManager.Instance.AddEffect(1, hit.point);

        if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("Breakable"))
        {
            var e = hit.collider.GetComponent<Enemy>();
            if (e)
            {
                e.Hurt(hit.point, dir.normalized);
                level.CheckEnd(touchTimer);
            }
        }

        if (hit.collider.gameObject.CompareTag("BatLimb"))
        {
            var limb = hit.collider.GetComponent<BatLimb>();
            limb.Break();
        }

        var line = linePool.Get();
        line.SetPosition(0, pos);
        line.SetPosition(1, hit.point);

        this.StartCoroutine(() => linePool.ReturnToPool(line), 0.1f);

        AudioManager.Instance.PlayEffectAt(0, pos, 1f);
        AudioManager.Instance.PlayEffectAt(1, pos, 0.7f);

        this.StartCoroutine(() =>
        {
            AudioManager.Instance.PlayEffectAt(14, hit.point, 0.898f);
            AudioManager.Instance.PlayEffectAt(21, hit.point, 0.694f);
            AudioManager.Instance.PlayEffectAt(11, hit.point, 0.588f);
            AudioManager.Instance.PlayEffectAt(2, hit.point, 0.8f);
        }, 0.07f);
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

    private static void ThrowBody(Joint2D joint)
    {
        joint.enabled = false;
        var dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
        joint.attachedRigidbody.AddForce(dir * 15f, ForceMode2D.Impulse);
    }

    public bool Hurt()
    {
        level.GotDamaged();
        flasher.Flash();

        hp--;

        if (hp <= 0)
        {
            Die();
            return true;
        }

        this.StartCoroutine(() =>
        {
            var position = transform.position;
            AudioManager.Instance.PlayEffectAt(8, position, 1f);
            AudioManager.Instance.PlayEffectAt(12, position, 0.767f);
            AudioManager.Instance.PlayEffectAt(14, position, 1.184f);
            AudioManager.Instance.PlayEffectAt(16, position, 0.384f);
            AudioManager.Instance.PlayEffectAt(11, position, 0.588f);
            AudioManager.Instance.PlayEffectAt(15, position, 0.457f);
        }, 0.1f);

        effectCam.BaseEffect(0.1f);

        return false;
    }

    public float GetTime()
    {
        return bestTime;
    }

    public void Heal()
    {
        hp = 3;
    }
}