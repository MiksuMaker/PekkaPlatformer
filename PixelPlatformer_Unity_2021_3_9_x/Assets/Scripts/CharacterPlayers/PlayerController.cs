using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Linq;

public class PlayerController : PlayerCharacter
{
    [Header("Player Audio clips")]
    public AudioClip JumpAudioClip;
    public AudioClip WarpPipeAudioClip;

    [Header("Player properties")]
    public CameraFollow cam;

    [Header("Reload map when completed (debug)")]
    public bool ReloadLoadedMap = false; //<-- set to true, to reload current scene in the end of the level...

    [HideInInspector]
    public WarpZone OnWarpZone = null;

    [HideInInspector]
    public LadderTile OnLadder = null;

    //Private properties that can be accessed only within this class    
    private int jumpCount = 0;
    private bool jumpStarted = false;
    private bool jumpButtonDown = false;
    private bool sliding = false;
    private bool ducking = false;
    private bool climbingLadder = false;    
    private bool climbingOnCoolDown = false;
    private bool warping = false;

    private LadderTile previousLadder = null;
    private Sequence pipeWarpSequence;

    private const float inputDeadZoneTreshold = 0.1f;           //input dead zone
    private const float inputDuckOrClimbTreshold = 0.4f;        //additional treshold for ducking input (so that accidental ducking is harder)
    private float horizontalInputValue = 0f;
    private float verticalInputValue = 0f;
    private float endSequenceFollowTime = 1f;    
    private float climbingCoolDownTime = 0.2f;                   //time in seconds for player to be able to climb ladders/ropes again after release
    private float ladderBottomY;
    private float ladderTopY;

    //Called on EndTile: start level ending sequence
    public void LevelCompleted()
    {
        UnSubscribeInput();
        StartCoroutine(LevelCompletedSequence());
    }

    public override void Die()
    {
        base.Die();
        audioSource.PlayOneShot(DieAudioClip);
    }

    public void UpdateInitPosition(Vector2 newInitPosition)
    {
        initPosition = newInitPosition;
    }

    public override void Reset()
    {
        base.Reset();

        //Instantly set camera to Player's PlayerSpawnPointPosition
        cam.WarpCameraPosition(transform.position);
    }

    protected void Awake()
    {
        cam.Init(this);
        cam.transform.SetParent(null);
    }

    protected override void HandleJumpInput(bool isDown)
    {
        if (isDown && (grounded != null || climbingLadder))
        {            
            if (climbingLadder)            
                StopClimbing(true);
            
            JumpStart();
            audioSource.PlayOneShot(JumpAudioClip);
        }
        else
            JumpEnd();

        jumpButtonDown = isDown;
    }

    //Use fixed time update with rigidbody physics
    protected override void FixedUpdate()
    {
        if (!IsAlive.Value)
            return;

        //Groundcheck from CharacterBase
        base.FixedUpdate();

        anim.speed = 1f;

        if (InputEnabled)
        {
            if (!climbingLadder && jumpButtonDown && jumpStarted)
                JumpContinue();

            horizontalInputValue = AxisInput.x;
            verticalInputValue = AxisInput.y;
        }
                
        //Duck and enter doors        
        VerticalMovement(verticalInputValue);

        if (climbingLadder)
            return;

        //Apply horizontal Force if no vertical                        
        if (!ducking)
            HorinzontalMovement(horizontalInputValue);
        
        //Flip sprite on horizontal movement
        FlipSprite(horizontalInputValue);

        //Slide in sloped terrain
        //if (OnWarpZone == null)
            sliding = Sliding(horizontalInputValue, verticalInputValue);

        //Set correct animation
        if (!warping)
            SetAnimation(Rb2D, horizontalInputValue, verticalInputValue, sliding); //<-- if vertical down
    }

    private void SetAnimation(Rigidbody2D rb, float horizontal, float vertical, bool sliding)
    {
        ducking = false;

        if (grounded != null)
        {
            
            SlowDownFactorX = initSlowDownFactor;

            //inputDuckTreshold ensures that ducking doesn't trigger accidently. Especially with analog sticks...
            if (vertical < -(inputDeadZoneTreshold + inputDuckOrClimbTreshold))
            {
                if (OnWarpZone != null)
                    TriggerAnimation("Pipe");
                else if (sliding)
                    TriggerAnimation("Slide");
                else
                {
                    TriggerAnimation("Duck");
                    ducking = true;
                    SlowDownFactorX = SlowDownFactorDuckingX;
                }
            }
            else if (Mathf.Abs(horizontal) > inputDeadZoneTreshold)
                TriggerAnimation("Walk");
            else
            {
                if (grounded.Angle == RayHitInfo.GroundAngle.Right30)
                    TriggerAnimation(spriteDirection == FacingDirection.Left ? "Slope30" : "Slope30Left");
                else if (grounded.Angle == RayHitInfo.GroundAngle.Right45)
                    TriggerAnimation(spriteDirection == FacingDirection.Left ? "Slope45" : "Slope45Left");
                else if (grounded.Angle == RayHitInfo.GroundAngle.Left30)
                    TriggerAnimation(spriteDirection == FacingDirection.Left ? "Slope30Left" : "Slope30");
                else if (grounded.Angle == RayHitInfo.GroundAngle.Left45)
                    TriggerAnimation(spriteDirection == FacingDirection.Left ? "Slope45Left" : "Slope45");
                else
                    TriggerAnimation("Idle");
            }
        }
        else
            TriggerAnimation("Jump");
    }
    
    private void StartClimbingLadder(LadderTile ladder)
    {
        previousLadder = ladder;
        ladderBottomY = ladder.transform.position.y + ladder.collider2d.offset.y;
        ladderTopY = ladder.transform.position.y + ladder.collider2d.bounds.extents.y * 2f - collider2d.size.y + ladder.collider2d.offset.y;

        //Zero out gravity while climbing
        Rb2D.gravityScale = 0;
        //Stop RigidBody
        SetRbVelocityZero();
        //Set character's pos to center of rope or ladder (level tile's pivot is on left, thus offset)
        Rb2D.position = new Vector2(OnLadder.transform.position.x + OnLadder.GetTileSideOffset(), Rb2D.position.y > ladderTopY ? ladderTopY : Rb2D.position.y );

        TriggerAnimation("ClimbLadder");

        climbingLadder = true;    
    }

    public void StopClimbing(bool jumpedOff = false)
    {
        climbingLadder = false;
        Rb2D.gravityScale = 1f;

        //Otherwise the re-grabbing is just too instant...
        if (!climbingOnCoolDown)
            StartCoroutine(ClimbingCoolDown(jumpedOff));
    }

    IEnumerator ClimbingCoolDown(bool jumped)
    {
        climbingOnCoolDown = true;

        //if player jumped off the ladder, wait till he's not touching the ladder anymore. Else wait for climbingCoolDownTime
        if (jumped)        
            while (previousLadder.CheckPlayerWithinBounds(transform.position))            
                yield return null;                    
        else
            yield return new WaitForSeconds(climbingCoolDownTime);

        climbingOnCoolDown = false;
        previousLadder = null;
    }

    private bool Sliding(float horizontal, float vertical)
    {
        if (grounded != null && grounded.Hit.normal.y < 1f)
        {
            SlowDownFactorX = 0.01f;

            if (vertical < -inputDeadZoneTreshold)
            {
                SlowDownFactorX = 1f;
                HorinzontalMovement(grounded.Hit.normal.x);
            }
            else
            {
                if (Mathf.Sign(horizontal) == Mathf.Sign(grounded.Hit.normal.x)) //<-- Down hill
                {
                    if (Mathf.Abs(horizontal) > inputDeadZoneTreshold)
                        SlowDownFactorX = SlowDownFactorDownhillX; //<-- introduce variable for down hill walking?!                        
                    else
                        SlowDownFactorX = 0;
                }
                else //<-- up hill
                {
                    if (Mathf.Abs(horizontal) > inputDeadZoneTreshold)
                    {
                        HorinzontalMovement(-grounded.Hit.normal.x);
                        SlowDownFactorX = initSlowDownFactor;
                    }
                    else
                        SlowDownFactorX = 0;
                }
            }
            return true;
        }
        else
        {
            SlowDownFactorX = initSlowDownFactor;
            return false;
        }
    }

    private void HorinzontalMovement(float inputH)
    {
        if (grounded != null)
        {
            Rb2D.AddForce(new Vector2(inputH * WalkForce * Time.deltaTime, 0), ForceMode2D.Impulse);            
        }
        else
            Rb2D.AddForce(new Vector2(inputH * InAirForce * Time.deltaTime, 0), ForceMode2D.Impulse);
    }

    private void VerticalMovement(float inputV)
    {
        if (climbingLadder)
        {
            MoveOnLadder(inputV);
            return;
        }

        if (inputV < 0) //Down actions
        {
            if (OnWarpZone != null && Mathf.Abs(verticalInputValue) > (inputDeadZoneTreshold + inputDuckOrClimbTreshold))
                WarpSequence(OnWarpZone);
        }
        else if (inputV > 0) //Up actions
        {            
            //Start climbing ladder/rope if on one.
            if (OnLadder != null)
            {
                if (!climbingLadder && !climbingOnCoolDown && verticalInputValue > (inputDeadZoneTreshold + inputDuckOrClimbTreshold) && previousLadder == null)
                    StartClimbingLadder(OnLadder);
            }

            //Enter doors and activate other stuff
        }
    }

    private void MoveOnLadder(float inputV)
    {
        anim.speed = 0;
        
        if (transform.position.y <= ladderBottomY && inputV < 0) //Drop from the bottom of the ladder
            StopClimbing();
        else if (transform.position.y > ladderTopY && inputV > 0) //Do nothing at the top of the ladder
            return;        
        else
        {
            //If user is pressing up or down, animate and move character on ladder
            if (inputV != 0)
            {
                anim.speed = 1f;
                Rb2D.MovePosition(Rb2D.position + new Vector2(0, inputV * 1f * Time.deltaTime));
            }
        }
    }

    private void JumpStart()
    {
        jumpStarted = true;
    }

    private void JumpEnd()
    {
        jumpStarted = false;
        jumpCount = 0;
    }

    private void JumpContinue()
    {
        if (jumpCount < MaxJumpCount)
            Rb2D.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

        jumpCount++;
    }

    private void WarpSequence(WarpZone warpZone)
    {
        warping = true;
        TriggerAnimation("Pipe");

        //NOTE: there are also other warps than pipe, is this really the right place to play warp sound
        audioSource.PlayOneShot(WarpPipeAudioClip);

        //Get WarpToPosition
        Vector2 warpToPosition = warpZone.WarpToZone.InOutPosition.position;
        //Disable input
        UnSubscribeInput();
        //Ignore all collisions
        IgnoreAllCollisions = true;
        //Stop RigidBody
        SetRbVelocityZero();
        //Set kinematic for animation
        Rb2D.isKinematic = true;
        //Set position to middle of pipe
        transform.position = Rb2D.position = new Vector2(warpZone.transform.position.x + warpZone.GetTileSideOffset(), Rb2D.position.y);
        
        float downToPipeHeight = .16f;

        //Animate with DoTween        
        pipeWarpSequence = DOTween.Sequence();
        pipeWarpSequence.Append(Rb2D.DOMove(Rb2D.position - new Vector2(0, downToPipeHeight), 1f).From(Rb2D.position, false));
        pipeWarpSequence.AppendCallback(() => cam.WarpCameraPosition(warpToPosition));
        pipeWarpSequence.Append(Rb2D.DOMove(warpToPosition, 1f).From(warpToPosition - new Vector2(0, downToPipeHeight), false));
        pipeWarpSequence.AppendCallback(() =>
        {
            //Enable input and collisions
            Rb2D.isKinematic = false;
            SubscribeToInput();
            IgnoreAllCollisions = false;
            warping = false;
            TriggerAnimation("Idle");
        });
    }

    //TODO: move this to ApplicationController...
    private IEnumerator LevelCompletedSequence()
    {
        //Disable whole input controller
        InputController.Instance.InputEnabled = false;

        AudioController.Instance.PlayTrack("WinJingle", 1f, .2f, false);

        float sequenceStartTime = Time.time;

        while (true)
        {
            //Basically the same if user was pressing 'right'
            horizontalInputValue = 1f;

            //Break after endSequenceFollowTime else yield FixedUpdate
            if (Time.time - sequenceStartTime > endSequenceFollowTime)
                break;
            else
                yield return new WaitForFixedUpdate();
        }

        //stop camera follow after endSequenceFollowTime
        cam.FollowPlayer = false;

        /*
        Return to main menu after end animations
        TODO: add proper level end screen with score and collected coins etc.
        */

        //Wait for Player to run off-screen
        yield return new WaitForSeconds(2.5f);

        //Wait and fade to black
        ApplicationController.Instance.uiController.DipToBlack(1f, 1f, .2f); // <- NOTE: kinda hackish uiController method call through app controller... Obfuscating.
        yield return new WaitForSeconds(1.1f);

        //re-enable cam follow
        cam.FollowPlayer = true;

        //enable input controller
        InputController.Instance.InputEnabled = true;

        //subscribe back to input
        SubscribeToInput();

        //Load world map or reload same map for debugging
        if (ReloadLoadedMap)
        {
            TriggerAnimation(initAnimationName, true);
            ApplicationController.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
            ApplicationController.LoadScene(GameHelper.WorldMap);
    }
}