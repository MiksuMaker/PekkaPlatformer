using UnityEngine;

public class Enemy : CharacterBase
{
    [Header("Generic Enemy Attributes")]
    public bool ChangeDirectionOnSideCollision = true;
    public bool ChangeDirectionOnAboutToFall = false;
    public bool TakeDamageOnStomp = true;
    public bool BounceFromBricks = true;
    public bool WakeUpOnCamViewport = true;

    protected override void Start()
    {
        //If enemy is always active
        if (!WakeUpOnCamViewport)
            IsActive.Value = true;

        base.Start();

        //Set Enemies to use Enemy layer
        gameObject.layer = GameHelper.EnemyLayer;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsAlive.Value)
            return;

        PlayerCharacter player = collision.gameObject.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            //Stompable enemy 
            if (TakeDamageOnStomp)
            {   
                if (Stomped(player.Rb2D.position, Rb2D.position))
                {
                    //Bounce player
                    player.Rb2D.velocity = new Vector2(player.Rb2D.velocity.x, 3f);                    
                    TakeDamage(player, TakeDamageType.Stomped);
                }
                else
                    player.TakeDamage(this);
            }
            else
                player.TakeDamage(this);
        }
    }
    
    protected override void FixedUpdate()
    {   
        //Activate enemy if transform.position point is within camera viewport
        if (!IsActive.Value && CameraHelper.IsPointWithinViewport(Camera.main, Camera.main.transform.position, transform.position))
            IsActive.Value = true;

        if (!IsAlive.Value || !IsActive.Value)
            return;

        //Ground/side checks from CharacterBase
        base.FixedUpdate();

        //Change walk direction on collision if ChangeDirectionOnSideCollision == true
        if (ChangeDirectionOnSideCollision && sideHit != null)                    
            ChangeDirOnSideCollision(sideHit);
        
        //Flip direction, if groundcheck fails on bottom left/right but is still grounded from center
        if (ChangeDirectionOnAboutToFall && grounded != null)
            ChangeDirOnFall(grounded);
    }

    protected virtual void ChangeDirOnFall(RayHitInfo groundHit)
    {
        if (!groundHit.BottomLeftGrounded && groundHit.BottomRightGrounded)
        {
            SetRbVelocityZero();
            facingDirection = FacingDirection.Right;
        }
        else if (groundHit.BottomLeftGrounded == true && groundHit.BottomRightGrounded == false)
        {
            SetRbVelocityZero();
            facingDirection = FacingDirection.Left;
        }
    }

    protected virtual void ChangeDirOnSideCollision(RayHitInfo sideHit)
    {        
        if (sideHit != null)
            facingDirection = sideHit.Type == RayHitInfo.HitType.Left ? FacingDirection.Right : FacingDirection.Left;
    }
}