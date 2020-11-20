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
    public DemoType type;
    public Letter letter;
    public List<Mover> bats;

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

        switch(type)
        {
            case DemoType.Intro:
                Intro();
                break;
            case DemoType.Kidnapping:
                Kidnapping();
                break;
        }
    }

    void Intro()
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

    void Kidnapping()
    {
        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            zoomer.ZoomTo(4);
            demo.MoveCamTo(new Vector3(-7f, 0f, 0f), 0.75f);
        }, 0.75f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            moon.SetTrigger("Jump");
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Hmm, what's (going on) over there...", true);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.MoveCamTo(new Vector3(2f, 0f, 0f), 0.5f);
            zoomer.ZoomTo(7f);
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Hey guys, have you seen...", false);
        }, 0.4f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            MoveBat(0, Vector3.right * 15f + Vector3.up, 0.3f, 0.1f);
            MoveBat(1, Vector3.right * 17f, 0.35f, 0.15f);
            MoveBat(2, Vector3.right * 14f + Vector3.up * 0.5f, 0.4f, 0.25f);
            MoveBat(3, Vector3.right * 16f + Vector3.down, 0.3f, 0.3f);
        }, 0.3f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Sigh...", false);
        }, 0.7f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            var bling = EffectManager.Instance.AddEffect(6, letter.transform.position);
            bling.transform.localScale = Vector3.one * 0.5f;
        }, 0.9f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("They seem to have (dropped) something!", false);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
            var pos = demo.moon.transform.position + Vector3.right * 7f;
            Tweener.Instance.MoveTo(demo.moon.transform, pos, 0.9f, 0, TweenEasings.BounceEaseOut);
            demo.MoveCamTo(new Vector3(6f, 0f, 0f), 1f);
        }, 1.1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moon.SetTrigger("Jump");
            Tweener.Instance.ScaleTo(letter.transform, Vector3.one, 1.2f, 0, TweenEasings.BounceEaseOut);
            Tweener.Instance.MoveTo(letter.transform, new Vector3(9f, 1.5f, 0), 0.8f, 0, TweenEasings.BounceEaseOut);
        }, 1.2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            letter.Open();
        }, 4f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Oh (hell) no!", true);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            SceneChanger.Instance.ChangeScene("Main");
            demo.moonBubble.Hide();
        }));
    }

    void MoveBat(int index, Vector3 dir, float duration, float delay)
    {
        this.StartCoroutine(() =>
        {
            bats[index].enabled = false;
            Tweener.Instance.MoveFor(bats[index].transform, dir, duration, 0, TweenEasings.QuadraticEaseInOut);
        }, delay);
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

public enum DemoType
{
    Intro,
    Kidnapping
}