using System;
using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    public AnimationCurve[] customEasings;
    private List<TweenAction> actions;

    public static Tweener Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        actions = new List<TweenAction>();
    }

    private void Update()
    {
        for (var i = actions.Count - 1; i >= 0; i--)
            if (actions[i].Process())
                actions.RemoveAt(i);
    }

    private TweenAction AddTween(Transform obj, Vector3 target, TweenAction.Type type, float duration, float delay,
        Func<float, float> ease, int easeIndex = -1, bool removeOld = true)
    {
        // remove old ones of same object
        if (removeOld)
            for (var i = actions.Count - 1; i >= 0; i--)
                if (actions[i].theObject == obj && actions[i].type == type)
                    actions.RemoveAt(i);

        var act = new TweenAction
        {
            type = type,
            theObject = obj,
            targetPos = target,
            tweenPos = 0f,
            tweenDuration = duration,
            tweenDelay = delay,
            customEasing = easeIndex
        };
        actions.Add(act);

        act.easeFunction = ease;

        return act;
    }

    public void MoveTo(Transform obj, Vector3 target, float duration, float delay, Func<float, float> ease = null,
        int easeIndex = -1, bool removeOld = true)
    {
        if (ease == null) ease = TweenEasings.LinearInterpolation;

        var act = AddTween(obj, target, TweenAction.Type.Position, duration, delay, ease, easeIndex, removeOld);
        act.startPos = act.theObject.position;
        StartCoroutine(act.SetStartPos());
    }

    public void MoveLocalTo(Transform obj, Vector3 target, float duration, float delay, Func<float, float> ease = null,
        int easeIndex = -1, bool removeOld = true)
    {
        if (ease == null) ease = TweenEasings.LinearInterpolation;

        var act = AddTween(obj, target, TweenAction.Type.LocalPosition, duration, delay, ease, easeIndex, removeOld);
        act.startPos = act.theObject.localPosition;
        StartCoroutine(act.SetStartLocalPos());
    }

    public void MoveFor(Transform obj, Vector3 target, float duration, float delay, Func<float, float> ease = null,
        int easeIndex = -1, bool removeOld = true)
    {
        MoveTo(obj, obj.position + target, duration, delay, ease, easeIndex, removeOld);
    }

    public void MoveLocalFor(Transform obj, Vector3 target, float duration, float delay, Func<float, float> ease = null,
        int easeIndex = -1, bool removeOld = true)
    {
        MoveLocalFor(obj, obj.localPosition + target, duration, delay, ease, easeIndex, removeOld);
    }

    public void RotateTo(Transform obj, Quaternion rotation, float duration, float delay,
        Func<float, float> ease = null, int easeIndex = -1, bool removeOld = true)
    {
        if (ease == null) ease = TweenEasings.LinearInterpolation;

        var act = AddTween(obj, Vector3.zero, TweenAction.Type.Rotation, duration, delay, ease, easeIndex, removeOld);
        act.startRot = act.theObject.rotation;
        act.targetRot = rotation;
        StartCoroutine(act.SetStartRot());
    }

    public void RotateFor(Transform obj, Quaternion rotation, float duration, float delay,
        Func<float, float> ease = null, int easeIndex = -1, bool removeOld = true)
    {
        RotateTo(obj, obj.rotation * rotation, duration, delay, ease, easeIndex, removeOld);
    }

    public void ScaleTo(Transform obj, Vector3 target, float duration, float delay, Func<float, float> ease = null,
        int easeIndex = -1, bool removeOld = true)
    {
        if (ease == null) ease = TweenEasings.LinearInterpolation;

        var act = AddTween(obj, target, TweenAction.Type.Scale, duration, delay, ease, easeIndex, removeOld);
        StartCoroutine(act.SetStartScale());
    }

    public void ColorTo(SpriteRenderer obj, Color color, float duration, float delay, Func<float, float> ease = null,
        int easeIndex = -1, bool removeOld = true)
    {
        if (ease == null) ease = TweenEasings.LinearInterpolation;

        var act = AddTween(obj.transform, Vector3.zero, TweenAction.Type.Color, duration, delay, ease, easeIndex,
            removeOld);
        act.sprite = obj;
        act.startColor = act.sprite.color;
        act.targetColor = color;
        StartCoroutine(act.SetStartColor());
    }
}