using UnityEngine;
using UniRx;

public class CharacterBase : PlatformerObject
{
    public enum FacingDirection { Left = -1, Right = 1 };
    public enum TakeDamageType { Undefined, Stomped, Shot, Burned };

    public bool DebuggingMode = true;

    [Header("Basic Attributes")]
    public string CharacterName;
    public int HitPoints = 1;
    public int DealDamage = 1;
    public bool LeftFacingSprite = false; //<-- is sprite drawn facing left?
    public bool GroundCheckEdges = false; //<-- instead of just checking ground from bottom center, check left/right edges as well
    [Tooltip("Use RigidBody velocity to flip sprite direction")]
    public bool FlipSpriteOnRbVelocity = true;
    [Tooltip("Layers that are ignored in side check raycasts")]
    public LayerMask SideRaycastIgnoreLayers;
    [Tooltip("Layers that are ignored in ground check raycasts")]
    public LayerMask GroundRaycastIgnoreLayers;
    [Tooltip("Set to false if character is not supposed to die in KillZones")]
    public bool DieInKillZone = true;
    [Header("Basic Movement")]
    [Tooltip("Single jump force")]
    public float JumpForce = 2f;
    [Tooltip("Max times force is added when jumping")]
    public int MaxJumpCount = 5;
    [Tooltip("Walk force")]
    public float WalkForce = 15f;
    [Tooltip("In air movement force")]
    public float InAirForce = 5f;

    [Tooltip("Actual distance to ground")]
    public float GroundedOffset = 0.04f;
    [Tooltip("Smaller offset for straight one-way platforms")]
    public float OneWayGroundedOffset = 0.01f;
    [Tooltip("Left/right raycast offset")]
    public float SideOffset = 0.04f;
    [Tooltip("Fake friction")]
    public float SlowDownFactorX = 0.95f;
    [Tooltip("Friction downhill")]
    public float SlowDownFactorDownhillX = 0.88f;
    [Tooltip("Friction ducking")]
    public float SlowDownFactorDuckingX = 1f;
    [Tooltip("Fake drag")]
    public float InAirSlowDownFactorX = 0.95f;
    [Tooltip("Limit physics?")]
    public bool LimitPhysics = true;
    [Tooltip("Limit physics on x and y axes")]
    public Vector2 MaxVelocity = new Vector2(5f, 10f);
    [Tooltip("Start facing direction (default left)")]
    public FacingDirection facingDirection = FacingDirection.Left;
    
    [HideInInspector]
    public Rigidbody2D Rb2D;

    [HideInInspector]
    public ReactiveProperty<bool> IsAlive = new ReactiveProperty<bool>(true);
    [HideInInspector]
    public ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>(false);

    [HideInInspector]
    public RayHitInfo grounded = null;

    [HideInInspector]
    public RayHitInfo sideHit = null;

    [HideInInspector]
    public BoxCollider2D collider2d { get; private set; }

    protected Animator anim;    
    protected string currentAnimationName = "Idle";
    protected FacingDirection spriteDirection;

    protected float initSlowDownFactor;
    private int initHP;
    private int initDealDamage;
    private int initLayer;
    private FacingDirection initFacingDirection;
    private FacingDirection initSpriteDirection;
    protected string initAnimationName;

    protected override void Start()
    {
        base.Start();
        
        //Get rigidbody (2D) component
        Rb2D = GetComponent<Rigidbody2D>();
        if (!Rb2D.freezeRotation)
            Debug.LogError("Pixels don't rotate. Is this intentional?");

        //Get animator component in parent...
        anim = GetComponent<Animator>();

        //or any child
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        
        //Get collider component (in this case capsule collider 2D)
        collider2d = GetComponent<BoxCollider2D>();

        spriteDirection = facingDirection;

        //Get init values        
        initHP = HitPoints;
        initDealDamage = DealDamage;
        initFacingDirection = facingDirection;
        initSpriteDirection = spriteDirection;
        initLayer = gameObject.layer;
        initSlowDownFactor = SlowDownFactorX;
        initAnimationName = currentAnimationName;
    }

    //Use fixed time update with rigidbody physics
    protected virtual void FixedUpdate()
    {
        //If character's collider is touching ground
        grounded = GroundCheck(GroundCheckEdges);

        sideHit = SideCheck();

        //Limit physics velocity
        if (LimitPhysics)
            LimitVelocity(Rb2D);

        //Flip characters's sprite depending on velocity x (if allowed)
        if (FlipSpriteOnRbVelocity)
            FlipSprite(Rb2D.velocity.x);
    }
    
    public virtual void TakeDamage(CharacterBase damageDealer, TakeDamageType damageType = TakeDamageType.Undefined)
    {
        HitPoints -= damageDealer.DealDamage;
        if (HitPoints <= 0)
        {
            HitPoints = 0;
            Die();
            //NOTE: stupid way to play stomp audioclip...
            if (damageType == TakeDamageType.Stomped && damageDealer is PlayerCharacter)
            {
                PlayerCharacter playerCharacter = damageDealer as PlayerCharacter;
                playerCharacter.PlayStompDamageAudio();
            }
        }
    }

    public virtual void Die()
    {
        StopAllCoroutines();

        if (DebuggingMode)
            Debug.Log("Character " + gameObject.name + " died.");

        gameObject.layer = GameHelper.NoCollisionLayer;

        IsAlive.Value = false;

        //Stop
        SetRbVelocityZero();

        //If this instance's derived type is PlayerCharacter, disable input
        if (this is PlayerCharacter) {
            PlayerCharacter p = this as PlayerCharacter;
            p.UnSubscribeInput();
            //Players also die with pre-made animation, so set kinematic
            p.Rb2D.isKinematic = true;
        }
    }

    //Side offset for top edge checking
    public Vector2 GetCharacterSideOffset()
    {
        return new Vector2(collider2d.size.x * .5f, 0);
    }

    public RayHitInfo TopCheck(bool checkTopLeftRight, bool reverseCast = false)
    {
        Vector2 raycastFrom = new Vector2(transform.position.x, transform.position.y + collider2d.size.y * .5f);
        float raycastLength = collider2d.size.y * .5f + GroundedOffset;
        Ray2D ray = new Ray2D(raycastFrom, Vector2.up);

        if (reverseCast)
        {
            ray.origin = ray.GetPoint(raycastLength);
            ray.direction = -ray.direction;
        }

        if (DebuggingMode)
        {
            Debug.DrawRay(ray.origin - GetCharacterSideOffset(), ray.direction * raycastLength, Color.yellow);
            Debug.DrawRay(ray.origin + GetCharacterSideOffset(), ray.direction * raycastLength, Color.magenta);
        }
        
        RaycastHit2D hit;

        //Assume that top center hits
        RayHitInfo.HitType hitType = RayHitInfo.HitType.Top;
        hit = Physics2D.Raycast(ray.origin, ray.direction, raycastLength);

        RayHitInfo hitInfo = new RayHitInfo(hit, hitType);

        //Do top LEFT raycast if center didn't hit and bottom edge check is used
        if (checkTopLeftRight)
        {            
            RaycastHit2D leftTopHit = Physics2D.Raycast(ray.origin - GetCharacterSideOffset(), ray.direction, raycastLength); //-sideoffset = left            
            if (leftTopHit.collider != null)
            {
                hit = leftTopHit;
                hitType = RayHitInfo.HitType.TopLeft;                
            }
        }

        //Do top RIGHT raycast if center didn't hit and bottom edge check is used
        if (checkTopLeftRight)
        {
            RaycastHit2D rightTopHit = Physics2D.Raycast(ray.origin + GetCharacterSideOffset(), ray.direction, raycastLength); //+sideoffset = right            
            if (rightTopHit.collider != null)
            {
                hit = rightTopHit;
                hitType = RayHitInfo.HitType.TopRight;                
            }
        }
        
        //Update info
        hitInfo.Hit = hit;
        hitInfo.Type = hitType;

        
        //Return null or RayHitInfo
        return hit.collider == null || (hit.collider != null && hit.collider.isTrigger) ? null : hitInfo;
    }

    public RayHitInfo GroundCheck(bool checkBottomLeftRight)
    {
        Vector2 raycastFrom = new Vector2(transform.position.x, transform.position.y + collider2d.size.y * .5f);        
        float raycastLength = collider2d.size.y * .5f + GroundedOffset;

        if (DebuggingMode)
        {
            Debug.DrawRay(raycastFrom, -Vector2.up * raycastLength, Color.blue);
            Debug.DrawRay(raycastFrom - GetCharacterSideOffset(), -Vector2.up * raycastLength, Color.cyan);
            Debug.DrawRay(raycastFrom + GetCharacterSideOffset(), -Vector2.up * raycastLength, Color.yellow);
        }

        RaycastHit2D hit;

        //Assume that bottom hits
        RayHitInfo.HitType hitType = RayHitInfo.HitType.Bottom;

        hit = Physics2D.Raycast(raycastFrom, -Vector2.up, raycastLength);

        RayHitInfo hitInfo = new RayHitInfo(hit, hitType);

        hitInfo.BottomLeftGrounded = hitInfo.BottomRightGrounded = false;

        //Do bottom LEFT raycast if center didn't hit and bottom edge check is used
        if (checkBottomLeftRight)
        {
            RaycastHit2D leftBottomHit = Physics2D.Raycast(raycastFrom - GetCharacterSideOffset(), -Vector2.up, raycastLength, ~GroundRaycastIgnoreLayers); //-sideoffset = left            
            if (leftBottomHit.collider != null)
            {
                hit = leftBottomHit;
                hitType = RayHitInfo.HitType.BottomLeft;
                hitInfo.BottomLeftGrounded = true;
            }
        }
        
        //Do bottom RIGHT raycast if center didn't hit and bottom edge check is used
        if (checkBottomLeftRight)
        {
            RaycastHit2D rightBottomHit = Physics2D.Raycast(raycastFrom + GetCharacterSideOffset(), -Vector2.up, raycastLength, ~GroundRaycastIgnoreLayers); //+sideoffset = right            
            if (rightBottomHit.collider != null)
            {
                hit = rightBottomHit;
                hitType = RayHitInfo.HitType.BottomRight;
                hitInfo.BottomRightGrounded = true;
            }
        }

        if (hit.collider != null)
        {
            if (hit.normal.x > 0 && hit.normal.y < .8f) //45 degree angle right (there's only 45 and 30 deg tiles)            
                hitInfo.Angle = RayHitInfo.GroundAngle.Right45;
            else if (hit.normal.x > 0 && hit.normal.y < 1f) //30 deg angle right       
                hitInfo.Angle = RayHitInfo.GroundAngle.Right30;
            else if (hit.normal.x < 0 && hit.normal.y < .8f) //45 degree angle left
                hitInfo.Angle = RayHitInfo.GroundAngle.Left45;
            else if (hit.normal.x < 0 && hit.normal.y < 1f) //30 deg angle left    
                hitInfo.Angle = RayHitInfo.GroundAngle.Left30;
            else
                hitInfo.Angle = RayHitInfo.GroundAngle.Straight;

            //Check that coordinates are actually above the platform in case of one-way platforms
            if (hit.collider.GetComponent<PlatformEffector2D>() != null)
            {
                if (Rb2D.position.y + OneWayGroundedOffset < hit.collider.bounds.max.y)                
                    return null;
            }
        }

        //Update info
        hitInfo.Hit = hit;
        hitInfo.Type = hitType;
        //Return null or RayHitInfo
        return hit.collider == null || (hit.collider != null && hit.collider.isTrigger) ? null : hitInfo;
    }

    protected RayHitInfo SideCheck()
    {
        //From mid center
        Vector2 raycastFrom = new Vector2(transform.position.x, transform.position.y + collider2d.size.y * .5f);

        //Half-width & offset
        float raycastLength = collider2d.size.x * .5f + SideOffset;

        RaycastHit2D hit;

        //Assume that left hits
        RayHitInfo.HitType hitType = RayHitInfo.HitType.Left;

        if (DebuggingMode)
        {
            Debug.DrawRay(raycastFrom, Vector2.left * raycastLength, Color.red);
            Debug.DrawRay(raycastFrom, Vector2.right * raycastLength, Color.green);
        }

        //Left raycast - ignore layers set in (inverted) RaycastIgnoreLayers 
        hit = Physics2D.Raycast(raycastFrom, Vector2.left, raycastLength, ~SideRaycastIgnoreLayers);

        //Right raycast if left didn't hit - ignore layers set in (inverted) RaycastIgnoreLayers 
        if (hit.collider == null)
        {
            hit = Physics2D.Raycast(raycastFrom, Vector2.right, raycastLength, ~SideRaycastIgnoreLayers);
            hitType = RayHitInfo.HitType.Right; //<-- if it was right instead, change value
        }

        //Return null or RayHitInfo
        return hit.collider == null || (hit.collider != null && hit.collider.isTrigger) ? null : new RayHitInfo(hit, hitType);
    }

    protected void SetRbVelocityZero()
    {
        Rb2D.velocity = Vector3.zero;
    }

    protected void LimitVelocity(Rigidbody2D rb)
    {
        Vector2 currentVelocity = rb.velocity;

        if (grounded != null)        
            currentVelocity.x *= SlowDownFactorX;        
        else        
            currentVelocity.x *= InAirSlowDownFactorX;
        
        if (Mathf.Abs(currentVelocity.x) > MaxVelocity.x)
            currentVelocity.x = Mathf.Sign(currentVelocity.x) * MaxVelocity.x;

        if (Mathf.Abs(currentVelocity.y) > MaxVelocity.y)
            currentVelocity.y = Mathf.Sign(currentVelocity.y) * MaxVelocity.y;

        rb.velocity = currentVelocity;
    }

    protected void FlipSprite(float velocityX)
    {
        if (velocityX == 0) return;
        spriteRenderer.transform.localScale = LeftFacingSprite? new Vector3(Mathf.Sign(-velocityX), 1f, 1f) : new Vector3(Mathf.Sign(velocityX), 1f, 1f);
        if (LeftFacingSprite)
            spriteDirection = spriteRenderer.transform.localScale.x < 0 ? FacingDirection.Left : FacingDirection.Right;
        else
            spriteDirection = spriteRenderer.transform.localScale.x < 0 ? FacingDirection.Right : FacingDirection.Left;
    }

    protected void TriggerAnimation(string animationName, bool overrideTrigger = false)
    {
        if (!overrideTrigger && animationName == currentAnimationName)
            return;

        anim.SetTrigger(animationName);
        currentAnimationName = animationName;
    }

    public virtual void AddImpulseForce(Vector2 direction, float addForce)
    {
        Rb2D.AddForce(direction * addForce, ForceMode2D.Impulse);
    }
        
    public override void Reset()
    {        
        //Restore position and set gameObject active
        base.Reset();

        //Restore init character values        
        HitPoints = initHP;
        DealDamage = initDealDamage;
        facingDirection = initFacingDirection;
        spriteDirection = initSpriteDirection;
        SlowDownFactorX = initSlowDownFactor;
        
        //Reset layer
        gameObject.layer = initLayer;

        //Set alive back to true
        IsAlive.Value = true;
        //Set active back to false
        IsActive.Value = false;
        //Set physics velocity to zero
        Rb2D.velocity = Vector2.zero;
        //Set grounded to null
        grounded = null;
        //Trigger initial animation (usually 'Idle')
        TriggerAnimation(initAnimationName, true);        
    }
}

public class RayHitInfo
{
    public enum HitType { Bottom, Left, Right, BottomLeft, BottomRight, TopLeft, TopRight, Top };
    public enum GroundAngle { Left30, Left45, Straight, Right30, Right45 };
    public RaycastHit2D Hit;
    public HitType Type;
    public GroundAngle Angle;
    public bool BottomLeftGrounded = false;
    public bool BottomRightGrounded = false;

    public RayHitInfo(RaycastHit2D hit, HitType type, GroundAngle groundAngle = GroundAngle.Straight)
    {
        Hit = hit;
        Type = type;
        Angle = groundAngle;
    }
}