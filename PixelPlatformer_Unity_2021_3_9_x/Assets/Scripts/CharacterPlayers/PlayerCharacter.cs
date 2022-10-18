using System.Collections;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerCharacter : CharacterBase {

    public MouseMovement MouseMove { get; private set; }

    //Coins and score as reactive properties for UI
    public ReactiveProperty<int> Coins { get; private set; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> Score { get; private set; } = new ReactiveProperty<int>(0);
    
    public bool IgnoreAllCollisions
    {
        get { return ignoreAllCollisions; }
        set {            
            ignoreAllCollisions = value;
            //Set PlayerCharacter to use IgnoreAllCollisions or Player layer
            gameObject.layer = ignoreAllCollisions ? GameHelper.IgnoreAllLayer : GameHelper.PlayerLayer;
        }
    }

    [Header("Basic Audio clips")]
    public AudioClip DieAudioClip;
    public AudioClip BreakTileAudioClip;
    public AudioClip DealStompDamageAudioClip;

    protected Vector2 AxisInput;
    protected bool InputEnabled = true;
    protected bool ignoreAllCollisions = false;
    protected AudioSource audioSource;

    private InputController inputController;
    private CompositeDisposable disposables = new CompositeDisposable();

    //Enable/disable character
    public void SetEnabled(bool isEnabled)
    {
        //NOTE: this is a bit obfuscating. SetEnabled is called every time scene is loaded and thus works. Also it's fucked to trust in some string animation name...
        //Prevent "Walk" animation locking to what is actually "Idle" in animator
        currentAnimationName = "Idle";

        InputEnabled = isEnabled;
        gameObject.SetActive(isEnabled);
    }

    public void ChangeCoinsAndScore(int deltaCoinsAmount, int deltaScoreAmount)
    {
        Coins.Value += deltaCoinsAmount;
        Score.Value += deltaScoreAmount;
    }

    //NOTE: not the greatest design pattern, audio needs to be handled differently
    public void PlayStompDamageAudio()
    {
        audioSource.PlayOneShot(DealStompDamageAudioClip);
    }

    protected override void Start()
    {
        base.Start();

        audioSource = GetComponent<AudioSource>();

        inputController = InputController.Instance;
        
        //Get input subscriptions
        SubscribeToInput();

        //Set PlayerCharacters to use Player layer
        gameObject.layer = GameHelper.PlayerLayer;
    }

    public void SubscribeToInput()
    {
        disposables = new CompositeDisposable();

        //Get input from input controller        
        inputController.Horizontal.Subscribe(horizontal => AxisInput.x = horizontal).AddTo(disposables);
        inputController.Vertical.Subscribe(vertical => AxisInput.y = vertical).AddTo(disposables);
        inputController.MouseMove.Subscribe(movement => MouseMove = movement).AddTo(disposables);        
        inputController.SelectAndJump.Subscribe(jump => HandleJumpInput(jump)).AddTo(disposables);

        InputEnabled = true;
    }

    //NOTE: this seems bit stupid, maybe refactor whole audio fx system to use AudioController...
    public void BreakTile()
    {
        audioSource.PlayOneShot(BreakTileAudioClip);
    }

    public void UnSubscribeInput()
    {        
        disposables.Dispose();
        InputEnabled = false;
    }

    void OnDestroy() {
        UnSubscribeInput();
    }
        
    protected virtual void HandleShooting(bool isDown) {}
    protected virtual void HandleJumpInput(bool isDown) {}

    public override void Reset()
    {                
        base.Reset();        
        Rb2D.isKinematic = false;
        SubscribeToInput();
    }

    public override void Die()
    {
        base.Die();
        StartCoroutine(DieSequence());
    }
    
    //NOTE: this is just an animation, scene resets in ApplicationController after a delay
    IEnumerator DieSequence()
    {
        //Bounce up a bit and then fall.       

        float bounceUpSpeed = 4f;
        float fallAddition = -0.2f;
        Vector2 position = transform.position;

        TriggerAnimation("Die"); //<-- trigger death sprite animation
        yield return new WaitForSeconds(.3f);

        while (true)
        {
            position.y += bounceUpSpeed * Time.fixedDeltaTime;
            bounceUpSpeed += fallAddition;

            transform.position = position;
            yield return null;
        }
    }
}
