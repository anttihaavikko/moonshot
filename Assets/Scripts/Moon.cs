﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Moon : MonoBehaviour
{
    public Rigidbody2D leftHand, rightHand;
    public Transform leftBarrel, rightBarrel;
    public Transform leftMuzzle, rightMuzzle;
    public Rigidbody2D body;
    public float amount = 1f;
    public EffectCamera effectCam;
    public LineRenderer linePrefab;
    public LinePool linePool;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = 10f;

        if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            leftHand.AddForce(leftHand.transform.right * amount, ForceMode2D.Impulse);
            var eff = EffectManager.Instance.AddEffect(0, leftMuzzle.position);
            eff.transform.parent = leftBarrel;
            effectCam.BaseEffect(0.2f);
            Shoot(leftBarrel.position, -leftHand.transform.right);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightHand.AddForce(-rightHand.transform.right * amount, ForceMode2D.Impulse);
            var eff = EffectManager.Instance.AddEffect(0, rightMuzzle.position);
            eff.transform.parent = rightBarrel;
            effectCam.BaseEffect(0.2f);
            Shoot(rightBarrel.position, rightBarrel.transform.right);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("Main");
        }
    }

    void Shoot(Vector3 pos, Vector3 dir)
    {
        var hit = Physics2D.Raycast(pos, dir);
        if(hit)
        {
            EffectManager.Instance.AddEffect(0, hit.point);
            EffectManager.Instance.AddEffect(1, hit.point);

            var line = linePool.Get();
            line.SetPosition(0, pos);
            line.SetPosition(1, hit.point);

            this.StartCoroutine(() => linePool.ReturnToPool(line), 0.1f);
        }
    }
}