using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EnemyRabbit : Enemy
{
    private CompositeDisposable disposables = new CompositeDisposable();

    // keep a copy of the jumpCoroutine
    private IEnumerator jumpCoroutine;

    protected override void Start()
    {
        base.Start();

        //To start/stop coroutine on IsActive value
        jumpCoroutine = JumpSequence();
        //Lifetime subscription to IsActive: start jumping if true
        IsActive.Subscribe(b => StartJumpSequence(b)).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!IsAlive.Value || !IsActive.Value)
            return;
        
        //Apply horinzontal forces
        HorinzontalMovement();        
    }

    public override void Die()
    {
        base.Die();

        StartCoroutine(DieSequence());
    }

    private void StartJumpSequence(bool b)
    {
        if (b) //<-- Start jumping
            StartCoroutine(jumpCoroutine);
        else //<-- Stop jumping
            StopCoroutine(jumpCoroutine);
    }

    IEnumerator JumpSequence()
    {
        while (IsAlive.Value)
        {
            TriggerAnimation("Jump", true);
            yield return new WaitForSeconds(1f);
            //AddImpulseForce(Vector2.up, 1f);            
            Rb2D.velocity = new Vector2(Rb2D.velocity.x, 2f);
        }
    }

    IEnumerator DieSequence()
    {
        TriggerAnimation("Die");
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    private void HorinzontalMovement()
    {
        if (grounded != null)
            Rb2D.AddForce(new Vector2((float)facingDirection * WalkForce * Time.deltaTime, 0), ForceMode2D.Impulse);
        else
            Rb2D.AddForce(new Vector2((float)facingDirection * InAirForce * Time.deltaTime, 0), ForceMode2D.Impulse);
    }
}
