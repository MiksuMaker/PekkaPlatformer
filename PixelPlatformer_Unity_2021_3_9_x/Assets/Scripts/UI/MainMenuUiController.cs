using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UniRx;

public class MainMenuUiController : UiBase
{
    public RectTransform LeftCurtain;
    public RectTransform RightCurtain;
    public MenuButton InitSelectedButton;
    public RectTransform StartMenuButtonsContainer;

    private Sequence CurtainsSequence;
    private RectTransform uiCanvasRect;
    private CompositeDisposable disposables = new CompositeDisposable();
    private List<MenuButton> mainMenuButtons = new List<MenuButton>();
    private MenuButton currentlySelectedButton;
    private MenuButton initSelectedButton;
    private float curtainScaledSidePos;
    private const float sideCurtainPosAnimAspect = 2.4f;
    private bool startingNewGame = false;

    public override void Show(bool show)
    {   
        base.Show(show);
        //Select first button again when scene is reloaded
        if (initSelectedButton != null)        
            initSelectedButton.Select();        
    }

    public void OpenCurtains()
    {
        curtainScaledSidePos = uiCanvasRect.rect.width / sideCurtainPosAnimAspect;
        
        CurtainsSequence = DOTween.Sequence();
        CurtainsSequence.Append(LeftCurtain.DOAnchorPosX(-curtainScaledSidePos, 2.5f, true));
        CurtainsSequence.Insert(0f, RightCurtain.DOAnchorPosX(curtainScaledSidePos, 2.5f, true));
        CurtainsSequence.OnComplete(CurtainsOpened);
    }

    private void Start()
    {
        LeftCurtain.gameObject.SetActive(true);
        RightCurtain.gameObject.SetActive(true);

        uiCanvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        mainMenuButtons = StartMenuButtonsContainer.GetComponentsInChildren<MenuButton>().ToList();
        initSelectedButton = mainMenuButtons.FirstOrDefault(button => button.InitiallySelect);
        
        //Subscribe to buttons' InternalSelected and Submitted and handle state changes
        mainMenuButtons.ForEach(button => button.Submitted.Subscribe(b => ButtonSubmittedHandler()).AddTo(disposables));
        mainMenuButtons.ForEach(button => button.InternalSelected.Subscribe(isSelected =>
        {
            if (isSelected)
                ButtonSelectedHandler(button);
        }
        ).AddTo(disposables));

        //Disable buttons in start
        EnableMenuButtons(false);
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    //Enable/disable menu buttons
    private void EnableMenuButtons(bool _enable)
    {
        mainMenuButtons.ForEach(button => button.ButtonComponent.interactable = _enable);
    }

    private void ButtonSubmittedHandler()
    {
        if (currentlySelectedButton.Type == MenuButton.ButtonType.OnePlayerNewGame)
        {
            if (!startingNewGame) //<- only start once
            {
                Debug.Log("Start new one player game.");
                startingNewGame = true;
                StartCoroutine(StartGameSequence());
            }
            
        }
        else
            Debug.LogError("This feature is not yet implemented.");
    }

    IEnumerator StartGameSequence()
    {
        AudioController.Instance.PlayTrack("IntroJingle", 1f, .3f, false);
        //Fade black in and then out
        ApplicationController.Instance.uiController.DipToBlack(1.5f, 1f, .2f); // <- NOTE: kinda hackish uiController method call through app controller... Obfuscating.
        yield return new WaitForSeconds(1.6f);

        //Starting back to false
        startingNewGame = false;

        //Load world map scene
        ApplicationController.LoadScene(GameHelper.WorldMap);                
    }

    private void ButtonSelectedHandler(MenuButton selectedButton)
    {
        currentlySelectedButton = selectedButton;
    }

    //Enable menu buttons after curtains have opened
    private void CurtainsOpened()
    {        
        EnableMenuButtons(true);

        //Select first button
        if (initSelectedButton != null)
            initSelectedButton.Select();
        else
            Debug.LogError("One of the StartMenuButtonControllers neeeds to be initially selected!");
    }
}
