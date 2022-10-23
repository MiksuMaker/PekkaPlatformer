using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EnemyStrawberry : Enemy
{
    private CompositeDisposable disposables = new CompositeDisposable();

    GameObject playerRef;       // TEHTAVAN KOODIA  1/5 !!!!!!!!!!!!!!!!!!!!!!!!!!!!
    bool canTurnAgain = true;
    float jumpDownTreshold = 0.5f;

    void OnDestroy()
    {
        disposables.Dispose();
    }

    protected override void Start()
    {
        base.Start();

        // Get a Player reference
        playerRef = FindObjectOfType<PlayerController>().gameObject;    // TEHTAVAN KOODIA  2/5 !!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!IsAlive.Value || !IsActive.Value)
            return;

        // Check for Player position    // TEHTAVAN KOODIA 3/5 !!!!!!!!!!!!!!!!!!!!!!!!!!!!
        CheckDirections();
        CheckAmbushOpportunity();

        //Apply horinzontal forces
        HorinzontalMovement();
    }

    // Turns the Enemy if Player is located behind them
    protected void CheckDirections()        // TEHTAVAN KOODIA 4/5 !!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        if (canTurnAgain)
        {
            if (facingDirection == FacingDirection.Left)
            {
                // Check if the Player is on the RIGHT side
                if (transform.position.x <= playerRef.transform.position.x)
                {
                    // Turn Right
                    facingDirection = FacingDirection.Right;
                    StartCoroutine(TurnTimer());
                }
            }
            else
            {
                // Check if the Player is on the LEFT side
                if (transform.position.x >= playerRef.transform.position.x)
                {
                    // Turn Right
                    facingDirection = FacingDirection.Left;
                    StartCoroutine(TurnTimer());
                }
            }
        }
    }

    // Makes the Enemy jump down from above platform if Player is below them
    protected void CheckAmbushOpportunity()        // TEHTAVAN KOODIA 5/5 !!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        // Check that the Player is below
        if (playerRef.transform.position.y < transform.position.y + 0.2f)
        {
            // Check if Player is within the Treshold (aka close enough)
            if (Mathf.Abs(playerRef.transform.position.x - transform.position.x) <= jumpDownTreshold)
            {
                // Jump down
                ChangeDirectionOnAboutToFall = false;
            }
            else
            {
                // Stay up
                ChangeDirectionOnAboutToFall = true;
            }
        }
        else
        {
            // Stay up
            ChangeDirectionOnAboutToFall = true;
        }
    }

    IEnumerator TurnTimer()     // TEHTAVAN KOODIA 6/5 !!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        canTurnAgain = false;

        // Wait a random amount time before allowing to turn again
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f,2f));

        canTurnAgain = true;
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
