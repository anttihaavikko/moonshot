using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public Animator moon, sun;
	public Bubble moonBubble, sunBubble;
    public EffectCamera effectCam;
    public Zoomer zoomer;
    public Transform cam;

    private Queue<DemoAction<Demo>> actions;

    // Start is called before the first frame update
    void Awake()
    {
        zoomer.ZoomTo(3, true);
    }

    private void Start()
    {
        Populate();
        Invoke("InvokeAction", 0.75f);
    }

    private void InvokeAction()
    {
        if(actions.Count > 0)
        {
            var a = actions.Dequeue();
            var delay = a.GetDelay();
            a.Invoke(this);

            if (delay >= 0f)
            {
                Invoke("InvokeAction", delay);
            }
        }
    }

    private void Populate()
    {
        actions = new Queue<DemoAction<Demo>>();

        PopulateIntro();
    }

    void PopulateIntro()
    {
        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            zoomer.ZoomTo(4);
            demo.MoveCamTo(new Vector3(-7f, 0f, 0f), 0.75f);
        }, 0.75f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Hiya, I'm (Monsieur Moon)!", true);
        }, 3f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
            demo.MoveCamTo(new Vector3(0f, 0f, 0f), 0.5f);
            zoomer.ZoomTo(5);
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("And this is my girlfriend, (Madame Sun)!", false);
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.sun.SetTrigger("Jump");
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
            demo.MoveCamTo(new Vector3(-7f, 0f, 0f), 0.5f);
            zoomer.ZoomTo(4);
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("I'm a bit of a gun nut...", true);
        }, 0.2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moon.SetTrigger("PullGun");
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("She isn't a big fan of them though...", true);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            Tweener.Instance.MoveTo(sun.transform, sun.transform.position + Vector3.right * 15f, 0.2f, 0f, TweenEasings.QuadraticEaseInOut);
            demo.MoveCamTo(new Vector3(0f, 0f, 0f), 0.5f);
            zoomer.ZoomTo(6);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Where did so go to?", false);
            zoomer.ZoomTo(5f);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Guess I gotta go (after her)...", false);
            demo.MoveCamTo(new Vector3(2f, 0f, 0f), 0.5f);
            zoomer.ZoomTo(4.5f);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            SceneChanger.Instance.ChangeScene("LevelSelect");
            demo.moonBubble.Hide();
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MoveCamTo(Vector3 pos, float duration)
    {
        Tweener.Instance.MoveTo(cam, pos, duration, 0f, TweenEasings.BounceEaseOut);
    }
}

public class DemoAction<T>
{
    private readonly System.Action<T> action;
    private readonly float delay;

    public DemoAction(System.Action<T> act, float delay = -1f) {
        action = act;
        this.delay = delay;
    }

    public void Invoke(T demo)
    {
        action.Invoke(demo);
    }

    public float GetDelay()
    {
        return delay;
    }
}