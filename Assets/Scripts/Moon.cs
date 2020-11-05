using UnityEngine;
using UnityEngine.SceneManagement;

public class Moon : MonoBehaviour
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

    private Level level;

    private Camera cam;
    private float touchTimer, bestTime = 0f;
    private bool hasTouched;

    private float autoShotDelay;
    private bool autoShoot = false;

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
            SceneManager.LoadSceneAsync("Main");
        }

        CheckTouch();
    }

    void CheckShots()
    {
        if (levelInfo.IsShown()) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (leftGun.activeSelf)
            {
                ShootLeft();
            }
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Alpha2) ||
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

            if(hit.collider.gameObject.tag == "Enemy")
            {
                var e = hit.collider.GetComponent<Enemy>();
                e.Hurt(hit.point, dir.normalized);

                level.CheckEnd(touchTimer);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Flag" && level)
        {
            level.Complete();
        }

        if (collision.gameObject.tag == "Bubble")
        {
            var trigger = collision.GetComponent<BubbleTrigger>();
            if(!trigger.shown && !Manager.Instance.IsShown(trigger.message))
            {
                this.StartCoroutine(() => bubble.Show(trigger.message), trigger.delay);
                Manager.Instance.Add(trigger.message);
                trigger.shown = true;
            }
        }
    }
}