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
    public List<BatFace> batFaces;

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

        moonBubble.afterHide += Skip;
        sunBubble.afterHide += Skip;

        AudioManager.Instance.ChangeMusic(1, 0.5f, 0.5f, 0f);
    }

    void Skip()
    {
        Invoke("InvokeAction", 0.5f);
    }

    public void InvokeAction()
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
            case DemoType.Trapped:
                Trapped();
                break;
        }
    }

    void ChangeMusic()
    {
        AudioManager.Instance.ChangeMusic(0, 0.5f, 0.5f, 0f);
    }

    void Intro()
    {
        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.zoomer.ZoomTo(4);
            demo.MoveCamTo(new Vector3(-7f, 0f, 0f), 0.75f);
        }, 0.75f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Hiya, I'm (Monsieur Moon)!", true);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.MoveCamTo(new Vector3(0f, 0f, 0f), 0.5f);
            demo.zoomer.ZoomTo(5);
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("And this is my girlfriend, (Madame Sun)!", false);
            demo.sun.SetTrigger("Jump");
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.MoveCamTo(new Vector3(-7f, 0f, 0f), 0.5f);
            demo.zoomer.ZoomTo(4);
        }, 0.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("I'm a bit of a gun nut...", true);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moon.SetTrigger("PullGun");
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("She isn't a big fan of them though...", true);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            Tweener.Instance.MoveTo(demo.sun.transform, sun.transform.position + Vector3.right * 15f, 0.2f, 0f, TweenEasings.QuadraticEaseInOut);
            demo.MoveCamTo(new Vector3(0f, 0f, 0f), 0.5f);
            demo.zoomer.ZoomTo(6);
        }, 2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Where did so go to?", false);
            demo.zoomer.ZoomTo(5f);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Guess I gotta go (after her)...", false);
            demo.MoveCamTo(new Vector3(2f, 0f, 0f), 0.5f);
            demo.zoomer.ZoomTo(4.5f);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            SceneChanger.Instance.ChangeScene("LevelSelect");
            demo.moonBubble.Hide();
            ChangeMusic();
        }));
    }

    void Kidnapping()
    {
        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.zoomer.ZoomTo(4);
            demo.MoveCamTo(new Vector3(-7f, 0f, 0f), 0.75f);
        }, 0.75f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            moon.SetTrigger("Jump");
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Hmm, what's (going on) over there...", true);
        }));

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
        }, 0.85f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            MoveBat(0, Vector3.right * 20f + Vector3.up, 0.3f, 0.1f);
            MoveBat(1, Vector3.right * 23f, 0.35f, 0.15f);
            MoveBat(2, Vector3.right * 19f + Vector3.up * 0.5f, 0.4f, 0.25f);
            MoveBat(3, Vector3.right * 21f + Vector3.down, 0.3f, 0.3f);
        }, 0.3f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("Sigh...", false);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            var bling = EffectManager.Instance.AddEffect(6, letter.transform.position);
            bling.transform.localScale = Vector3.one * 0.5f;
        }, 0.9f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.ShowWithMirroring("They seem to have (dropped) something!", false);
        }));

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
            Tweener.Instance.ScaleTo(demo.letter.transform, Vector3.one, 1.2f, 0, TweenEasings.BounceEaseOut);
            Tweener.Instance.MoveTo(demo.letter.transform, new Vector3(9f, 1.5f, 0), 0.8f, 0, TweenEasings.BounceEaseOut);
        }, 1.2f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.letter.Open();
        }, 2.5f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moon.SetTrigger("PullGun");
            demo.moonBubble.ShowWithMirroring("Oh (hell) no!", true);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            SceneChanger.Instance.ChangeScene("Main");
            ChangeMusic();
        }));
    }

    void Trapped()
    {
        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            MoveBat(0, Vector3.left * 5f, 0.3f, 0f);
            MoveBat(1, Vector3.left * 6f, 0.35f, 0f);
            MoveBat(2, Vector3.left * 7f, 0.4f, 0f);
            MoveBat(3, Vector3.left * 5f, 0.3f, 0f);

            demo.zoomer.ZoomTo(7);
            demo.MoveCamTo(new Vector3(3f, 0f, 0f), 0.75f);
        }, 0.75f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            moon.SetTrigger("Jump");
            demo.BatsLooksAt(demo.moon.transform.position);
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moon.SetTrigger("PullGun");
            demo.moonBubble.ShowWithMirroring("Keep coming at me you filthy sky rats!", true);
        }));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.BatsLooksAt(demo.moon.transform.position, 0.4f);
        }, 1f));

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            demo.moonBubble.Hide();
            MoveBatRandomish(0, demo.moon.transform.position, 1.5f);
            MoveBatRandomish(1, demo.moon.transform.position, 1.5f);
            MoveBatRandomish(2, demo.moon.transform.position, 1.5f);
            MoveBatRandomish(3, demo.moon.transform.position, 1.5f);
        }, 0.75f));

        for (var i = 0; i < 10; i++)
        {
            var step = i;
            actions.Enqueue(new DemoAction<Demo>((demo) =>
            {
                demo.effectCam.BaseEffect(0.3f);
                demo.zoomer.ZoomTo(7 - 0.3f * step);
                demo.MoveCamTo(new Vector3(3f - 0.7f * step, 0f, 0f), 0.2f);
                MoveBatRandomish(0, demo.moon.transform.position, 0.5f, true);
                MoveBatRandomish(1, demo.moon.transform.position, 0.5f, true);
                MoveBatRandomish(2, demo.moon.transform.position, 0.5f, true);
                MoveBatRandomish(3, demo.moon.transform.position, 0.5f, true);
                demo.BatsLooksAt(demo.moon.transform.position);
            }, 0.25f));
        }

        actions.Enqueue(new DemoAction<Demo>((demo) =>
        {
            SceneChanger.Instance.ChangeScene("Main");
            demo.moonBubble.Hide();
            ChangeMusic();
        }));
    }

    void BatsLooksAt(Vector3 pos, float amount = 0.2f)
    {
        batFaces.ForEach(bf => bf.LookAt(pos, amount));
    }

    void MoveBatRandomish(int index, Vector3 pos, float speed = 1f, bool doEffects = false)
    {
        this.StartCoroutine(() =>
        {
            bats[index].enabled = false;
            var dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            var p = pos + dir.normalized * Random.Range(0f, 1.5f);
            Tweener.Instance.MoveTo(bats[index].transform, p, Random.Range(0.25f, 0.5f) * speed, 0, TweenEasings.QuadraticEaseInOut);
            if(doEffects)
            {
                EffectManager.Instance.AddEffect(3, bats[index].transform.position);
                EffectManager.Instance.AddEffect(1, bats[index].transform.position);
            }
        }, Random.Range(0.1f, 0.3f) * speed);   
    }

    void MoveBat(int index, Vector3 dir, float duration, float delay)
    {
        this.StartCoroutine(() =>
        {
            bats[index].enabled = false;
            Tweener.Instance.MoveFor(bats[index].transform, dir, duration, 0, TweenEasings.QuadraticEaseInOut);
        }, delay);

        this.StartCoroutine(() =>
        {
            bats[index].SetOrigin();
            bats[index].enabled = true;
        }, delay + duration);
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
    Kidnapping,
    Trapped
}