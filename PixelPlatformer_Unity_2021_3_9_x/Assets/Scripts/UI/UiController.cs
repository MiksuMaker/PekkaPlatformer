using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class UiController : MonoBehaviour
{
    //Different ui part controllers
    public IngameUiController IngameUiController;
    public MainMenuUiController MainMenuUiController;
    public Image FadeBlackImage;

    //NOTE: I don't if this a good idea to manage audio clips...
    public AudioClip CoinAudioClip;

    private PlayerController playerController;
    private Sequence fadeBlackSequence;
    private IEnumerator enterMainMenuCoroutine;
    private AudioSource audioSource;

    
    public void Init(PlayerController _playerController)
    {
        audioSource = GetComponent<AudioSource>();
        playerController = _playerController;
        playerController.Coins.Subscribe(amount => HandleCoinsChange(amount));
    }

    public void SceneLoaded(string _sceneName)
    {
        //TODO: Music tracks shouldn't be played from here. Move music logic to app controller?
        switch (_sceneName)
        {
            case GameHelper.MainMenu:
                AudioController.Instance.PlayTrack("Menu", 1f, 1f);
                MainMenuUiController.Show(true);
                IngameUiController.Show(false);                
                EnterMainMenu();
                break;
            case GameHelper.WorldMap:
                AudioController.Instance.PlayTrack("WorldMap", 1f, .5f);
                MainMenuUiController.Show(false);
                IngameUiController.Show(false);
                break;
            default:
                bool trackIsOnPlayList = AudioController.Instance.PlayTrack(_sceneName, 1f, 1f);
                if (!trackIsOnPlayList)
                    Debug.LogError("Music track '" + _sceneName + "' is not on playlist!");

                MainMenuUiController.Show(false);
                IngameUiController.Show(true);
                break;
        }
    }

    public void EnterMainMenu()
    {
        //NOTE: for reasons known only to Unity developers, does not work with IEnumerator variable...
        StopCoroutine("EnterMainMenuSequence");        
        SetBlackImageAlpha(1f);
        FadeBlackImage.gameObject.SetActive(true);
        StartCoroutine("EnterMainMenuSequence");
    }

    IEnumerator EnterMainMenuSequence()
    {
        Debug.Log("Main menu sequence...");
        yield return new WaitForSeconds(1f);
        FadeBlackOut(1f);
        yield return new WaitForSeconds(1f);
        MainMenuUiController.OpenCurtains();
    }

    public void FadeBlackIn(float _time)
    {
        SetBlackImageAlpha(0);
        FadeBlackImage.gameObject.SetActive(true);
        FadeBlackImage.DOFade(1f, _time);
    }

    public void FadeBlackOut(float _time, float _fromAlpha = 1f)
    {
        SetBlackImageAlpha(1f);
        FadeBlackImage.gameObject.SetActive(true);
        FadeBlackImage.DOFade(0f, _time).OnComplete(() => FadeBlackImage.gameObject.SetActive(false));
    }

    public void DipToBlack(float _fadeInTime, float _fadeOutTime, float _stayTime = 0f)
    {
        SetBlackImageAlpha(0);
        FadeBlackImage.gameObject.SetActive(true);

        fadeBlackSequence = DOTween.Sequence();
        fadeBlackSequence.Append(FadeBlackImage.DOFade(1f, _fadeInTime));
        fadeBlackSequence.Insert(_fadeInTime + _stayTime, FadeBlackImage.DOFade(0f, _fadeOutTime));
        fadeBlackSequence.OnComplete(() => FadeBlackImage.gameObject.SetActive(false));
    }

    private void SetBlackImageAlpha(float _alpha)
    {
        Color currentImgCol = FadeBlackImage.color;
        currentImgCol.a = _alpha;
        FadeBlackImage.color = currentImgCol;
    }

    private void HandleCoinsChange(int currentCoins)
    {
        //NOTE: this will also play sound if coins are less than current. Refactor this.
        audioSource.PlayOneShot(CoinAudioClip);

        IngameUiController.UpdateTextCoins(currentCoins);
    }

    private void HandleTimeChange()
    {

    }
}
