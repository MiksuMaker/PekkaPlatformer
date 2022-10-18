using System.Collections;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class MapPlayer : MonoBehaviour
{
    public enum FacingDir { Left, Right, Up, Down };    
    public bool DebugMode = false;    

    private InputController inputController;
    private CompositeDisposable disposables = new CompositeDisposable();
    private bool InputEnabled = true;
    private Vector2 AxisRawInput;
    private Animator anim;
    private bool isMoving = false;
    private FacingDir currentDir;

    private WorldMapTile currentTile;

    private bool enteringLevel = false;

    void Start()
    {
        //Get or create input instance
        inputController = InputController.Instance;

        //Get animator
        anim = GetComponent<Animator>();

        //Get input subscriptions
        SubscribeToInput();
    }

    public void SubscribeToInput()
    {
        disposables = new CompositeDisposable();

        //Get input from input controller        
        inputController.HorizontalRaw.Subscribe(horizontal => AxisRawInput.x = horizontal).AddTo(disposables);
        inputController.VerticalRaw.Subscribe(vertical => AxisRawInput.y = vertical).AddTo(disposables);
        inputController.SelectAndJump.Subscribe(jump => HandleJumpInput(jump)).AddTo(disposables);

        InputEnabled = true;
    }

    public void UnSubscribeInput()
    {
        disposables.Dispose();
        InputEnabled = false;
    }

    private void FixedUpdate()
    {
        //No need to raycast if char still moving to another tile
        if (!isMoving)
        {            
            RayCastMapTile(AxisRawInput);
            anim.SetTrigger("Idle" + currentDir.ToString());
        }
        else        
            anim.SetTrigger("Walk" + currentDir.ToString());        
    }

    private void HandleJumpInput(bool isDown)
    {
        //As menu button, react when button is released
        if (!isDown && currentTile != null && currentTile.Type == WorldMapTile.TileType.Level)
        {
            if (!enteringLevel)
            {
                enteringLevel = true;
                StartCoroutine(StartGameSequence(currentTile.SelectedWorld, currentTile.SelectedLevel));
            }
        }
    }

    IEnumerator StartGameSequence(int world, int level)
    {
        //Fade black in and then out
        ApplicationController.Instance.uiController.DipToBlack(1f, 1f, .2f); // <- NOTE: kinda hackish uiController method call through app controller... Obfuscating.
        yield return new WaitForSeconds(1.1f);

        //Entering back to false
        enteringLevel = false;

        //Load correct level scene
        ApplicationController.LoadScene(GameHelper.GetLevelNameString(world, level));        
    }

    private void RayCastMapTile(Vector2 dir)
    {
        //Round vector to prevent diagonal movement because joystick input raw gives floats
        if (Mathf.Abs(dir.x) > 0.5f) dir.y = 0;
        else if (Mathf.Abs(dir.y) > 0.5f) dir.x = 0;

        if (DebugMode)
            Debug.DrawRay(transform.position, dir * GameHelper.TileUnitSize, Color.red, 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, GameHelper.TileUnitSize);
        if (hit.collider != null)
        {
            currentTile = hit.collider.GetComponent<WorldMapTile>();
            if (currentTile != null)
            {
                Vector3 tilePos = currentTile.transform.position;

                currentDir = GetNextFacingDirection(tilePos, transform.position);
                
                //Tween to raycast's target tile position
                isMoving = true;
                transform.DOMove(tilePos, .3f).SetEase(Ease.Linear).OnComplete(MovementCompleted);
            }
        }
    }

    private FacingDir GetNextFacingDirection(Vector3 tilePos, Vector3 characterPos)
    {
        if (tilePos.x > characterPos.x)
            return FacingDir.Right;
        else if (tilePos.x < characterPos.x)
            return FacingDir.Left;
        else if (tilePos.y > characterPos.y)
            return FacingDir.Up;
        else if (tilePos.y < characterPos.y)
            return FacingDir.Down;
        else
            return FacingDir.Down;
    }

    private void MovementCompleted()
    {
        isMoving = false;         
    }

    private void OnDestroy()
    {
        UnSubscribeInput();
    }
}
