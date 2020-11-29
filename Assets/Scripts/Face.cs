using UnityEngine;

public class Face : MonoBehaviour
{
    public enum Emotion
    {
        Default,
        Angry,
        Sad,
        Happy,
        Sneaky,
        Shocked,
        Brag
    }

    public Transform[] eyes;
    public Transform[] pupils;
    public Transform[] brows;
    public Transform mouth;
    public float blinkSpeed = 0.15f;
    public float blinkDelay = 3f;
    public float derpiness = 0.1f;

    public Vector2 faceRange = new Vector2(0.05f, 0.11f);
    public Vector2 pupilRange = new Vector2(0.2f, 0.2f);
    public float mouthCloseChange = 0.5f;
    public float mouthSpeed = 1f;
    public Vector3 mouthRange = new Vector3(0.1f, 0.1f, 1f);
    public float browRange = 0.05f;
    public Transform lookTarget;
    public float lookSpeed = 1f;
    public float emoteSpeed = 0.05f;

    public SpriteRenderer mouthSprite;
    public Sprite mouthAngry, mouthSad, mouthHappy, mouthOpen;

    public bool followMouse;
    public Camera cam;
    private float browDir = 1f;
    private float[] browsTargetAngle, browsOriginalAngle;

    private Vector3[] browsTargetPosition, browsOriginalPosition;
    private Sprite mouthDefault;
    private Vector3 mouthOriginalPos;
    private Vector3 mouthOrignalScale;
    private Vector3 mouthPos = Vector3.zero;
    private float mouthScale = 1f;
    private Vector3 mouthScaler = Vector3.one;

    private Emotion nextEmotion;

    private float size;

    // Use this for initialization
    private void Awake()
    {
        cam = Camera.main;

        if (brows.Length > 0)
        {
            browsTargetPosition = new Vector3[brows.Length];
            browsOriginalPosition = new Vector3[brows.Length];
            browsTargetAngle = new float[brows.Length];
            browsOriginalAngle = new float[brows.Length];

            for (var i = 0; i < brows.Length; i++)
            {
                browsTargetPosition[i] = browsOriginalPosition[i] = brows[i].localPosition;
                browsTargetAngle[i] = browsOriginalAngle[i] = brows[i].localRotation.z;
            }
        }

        if (mouth)
        {
            mouthOrignalScale = mouth.localScale;
            mouthOriginalPos = mouth.localPosition;
            mouthDefault = mouthSprite.sprite;
        }

        size = eyes[0].localScale.y;
    }

    private void Start()
    {
        Invoke("Blink", blinkDelay * Random.Range(0.8f, 1.2f));
    }

    // Update is called once per frame
    private void Update()
    {
        //MoveFace ();

        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Emote(Emotion.Default);

            if (Input.GetKeyDown(KeyCode.Alpha2)) Emote(Emotion.Angry);

            if (Input.GetKeyDown(KeyCode.Alpha3)) Emote(Emotion.Happy);

            if (Input.GetKeyDown(KeyCode.Alpha4)) Emote(Emotion.Shocked);

            if (Input.GetKeyDown(KeyCode.Alpha5)) Emote(Emotion.Sad);

            if (Input.GetKeyDown(KeyCode.Alpha6)) Emote(Emotion.Sneaky);

            if (Input.GetKeyDown(KeyCode.Alpha7)) Emote(Emotion.Brag);
        }

        if (brows.Length > 0)
            for (var i = 0; i < brows.Length; i++)
            {
                brows[i].localPosition = Vector3.MoveTowards(brows[i].localPosition, browsTargetPosition[i],
                    Time.deltaTime * 20f * emoteSpeed);
                brows[i].localRotation = Quaternion.RotateTowards(brows[i].localRotation,
                    Quaternion.Euler(0, 0, browsTargetAngle[i]), Time.deltaTime * 60f * 20f * emoteSpeed);
            }

        if (mouth)
        {
            mouthScale = 1f - Mathf.Abs(Mathf.Sin(Time.time * mouthSpeed)) * mouthCloseChange;
            var targetScale = new Vector3(mouthOrignalScale.x, mouthOrignalScale.y * mouthScale, mouthOrignalScale.z);
            mouth.localScale = Vector3.MoveTowards(mouth.localScale, Vector3.Scale(targetScale, mouthScaler),
                Time.deltaTime * emoteSpeed * 10f);
            mouth.localPosition = Vector3.MoveTowards(mouth.localPosition,
                mouthOriginalPos + Vector3.Scale(mouthPos, mouthRange), Time.deltaTime * 5f * emoteSpeed);
        }
    }

    public void RotateBrows(float left, float right)
    {
        if (brows.Length > 0)
        {
            browsTargetAngle[1] = browsOriginalAngle[1] + left;
            browsTargetAngle[0] = browsOriginalAngle[0] + right;
        }
    }

    public void MoveBrows(float left, float right)
    {
        if (brows.Length > 0)
        {
            browsTargetPosition[1] = browsOriginalPosition[1] + Vector3.up * browRange * left;
            browsTargetPosition[0] = browsOriginalPosition[0] + Vector3.up * browRange * right;
        }
    }

    public void ResetBrows()
    {
        MoveBrows(0f, 0f);
        RotateBrows(0f, 0f);
    }

    private void RaiseBrows()
    {
        MoveBrows(1f, 1f);
        RotateBrows(10f, -10f);
    }

    private void MoveFace()
    {
        var mp = Input.mousePosition;
        mp.z = 10f;
        var mouseInWorld = cam.ScreenToWorldPoint(mp);
        Vector2 lookPos = mouseInWorld - transform.parent.position;

        lookPos = Quaternion.Euler(new Vector3(0, 0, -transform.parent.rotation.eulerAngles.z)) * lookPos;

        transform.localPosition = Vector2.MoveTowards(transform.localPosition,
            Vector2.Scale(lookPos.normalized, faceRange), Time.deltaTime * lookSpeed);

        if (pupils.Length > 0)
            for (var i = 0; i < pupils.Length; i++)
                pupils[i].localPosition = Vector2.MoveTowards(pupils[i].localPosition,
                    Vector2.Scale(lookPos, pupilRange), Time.deltaTime * lookSpeed * 2f);
    }

    private void Blink()
    {
        foreach (var e in eyes)
        {
            var delay = Random.Range(0f, derpiness);
            Tweener.Instance.ScaleTo(e, new Vector3(e.localScale.x, e.localScale.x * 0.1f, e.localScale.z), blinkSpeed,
                delay, TweenEasings.QuarticEaseOut);
            Tweener.Instance.ScaleTo(e, new Vector3(e.localScale.x, e.localScale.x, e.localScale.z), blinkSpeed,
                blinkSpeed + delay, TweenEasings.QuarticEaseOut, -1, false);
        }

        Invoke("Blink", blinkDelay * Random.Range(0.8f, 1.2f));
    }

    private void ChangeMouth(Sprite sprite)
    {
        ChangeMouth(sprite, Vector3.one, Vector3.zero);
    }

    private void ChangeMouth(Sprite sprite, Vector3 scale, Vector3 pos)
    {
        if (sprite)
            mouthSprite.sprite = sprite;
        else
            mouthSprite.sprite = mouthDefault;

        mouthScaler = scale;
        mouthPos = pos;
    }

    public void Emote(Emotion emotion, Emotion next, float delay)
    {
        nextEmotion = next;
        Emote(emotion);
        Invoke("DoNextEmote", delay);
    }

    private void DoNextEmote()
    {
        Emote(nextEmotion);
    }

    public void Emote(Emotion emotion)
    {
        CancelInvoke("DoNextEmote");

        if (emotion == Emotion.Default)
        {
            ResetBrows();
            ChangeMouth(mouthDefault);
        }

        if (emotion == Emotion.Angry)
        {
            RotateBrows(-20f, 20f);
            MoveBrows(-0.75f, -0.75f);
            ChangeMouth(mouthAngry);
        }

        if (emotion == Emotion.Happy)
        {
            RotateBrows(5f, -5f);
            MoveBrows(0.25f, 0.25f);
            ChangeMouth(mouthHappy, new Vector3(1.25f, 1.5f, 1f), new Vector3(0, -0.5f, 0f));
        }

        if (emotion == Emotion.Shocked)
        {
            RotateBrows(3f, -3f);
            MoveBrows(0.5f, 0.5f);
            ChangeMouth(mouthOpen, new Vector3(1.25f, 1.25f, 1f), new Vector3(0, -0.25f, 0f));
        }

        if (emotion == Emotion.Sad)
        {
            RotateBrows(25f, -25f);
            MoveBrows(0.25f, 0.25f);
            ChangeMouth(mouthSad);
        }

        if (emotion == Emotion.Sneaky)
        {
            browDir = -browDir;

            RotateBrows(-10f, 10f);
            MoveBrows(browDir, -browDir);
            ChangeMouth(mouthHappy, new Vector3(0.8f, 1f, 1f), new Vector3(0.5f * Random.Range(-1f, 1f), -0.2f, 0f));
        }

        if (emotion == Emotion.Brag)
        {
            RaiseBrows();
            Invoke("ResetBrows", 0.15f);
            Invoke("RaiseBrows", 0.35f);
            Invoke("ResetBrows", 0.5f);

            nextEmotion = Emotion.Default;
            Invoke("DoNextEmote", 1f);

            ChangeMouth(mouthHappy, new Vector3(1.3f, 1.1f, 1f), new Vector3(0, 0.1f, 0f));
        }
    }
}