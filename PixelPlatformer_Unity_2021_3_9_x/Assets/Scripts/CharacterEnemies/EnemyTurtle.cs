using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EnemyTurtle : Enemy
{
    private CompositeDisposable disposables = new CompositeDisposable();
    
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
    
    IEnumerator DieSequence()
    {
        TriggerAnimation("Die");
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    private void HorinzontalMovement()
    {
        if (grounded != null)
        {
            TriggerAnimation("Walk");
            Rb2D.AddForce(new Vector2((float)facingDirection * WalkForce * Time.deltaTime, 0), ForceMode2D.Impulse);
        }
        else
        {
            TriggerAnimation("Idle");
            Rb2D.AddForce(new Vector2((float)facingDirection * InAirForce * Time.deltaTime, 0), ForceMode2D.Impulse);
        }
    }
}
