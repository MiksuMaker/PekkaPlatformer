using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UniRx;

public class ApplicationController : SingletonMono<ApplicationController>
{
    [HideInInspector]
    public UiController uiController;

    [HideInInspector]
    public int CurrentLevelTimeLeft = 300; //seconds

    public PlayerController player { get; private set; }

    private CameraFollow cam;    
    private List<PlatformerObject> resetables;
    private bool resetting = false;
    private string currentSceneName;
    private CompositeDisposable disposables;

    private bool ignoreFirstCancel = true;

    [RuntimeInitializeOnLoadMethod]
    static void OnInit()
    {
        Instance.Init();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {        
        Debug.Log("Scene loaded: " + loadedScene.name);

        InputController.Instance.InputEnabled = true; //Enable input controller
        currentSceneName = loadedScene.name; //Store current scene's name        
        player.SetEnabled(currentSceneName != GameHelper.MainMenu && currentSceneName != GameHelper.WorldMap); //Disable player in main menu and world map        
        uiController.SceneLoaded(currentSceneName); //Tell UI what to do with recently loaded scene
        cam.UpdateCameraConstraints(); //Update camera constraints for loaded scene
        GetSceneResetables(); //Get and update resetable objects from loaded scene

        PlayerSpawnPoint spawnPoint = GetPlayerSpawnPoint();
        player.UpdateInitPosition(spawnPoint.transform.position); //Update initPosition to new scene's spawnPoint for reset
        PlayerToSpawnPoint(spawnPoint); //Set Player to scene's spawn point
    }

    private void PlayerToSpawnPoint(PlayerSpawnPoint playerSpawnPoint)
    {        
        player.transform.position = playerSpawnPoint.transform.position;
        cam.WarpCameraPosition(playerSpawnPoint.transform.position);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        disposables.Dispose();
    }

    public void Init()
    {
        //Instantiate and assign values for player and UI prefabs from Resources
        player = InstantiatePlayer();
        uiController = InstantiateUiPrefab();
        //Get player cam reference
        cam = player.cam;

        //Player, his camera and ui are persistent, don't destroy on load
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(uiController);
        DontDestroyOnLoad(cam);

        if (player != null)
        {
            //Init UiController: pass player reference(s)        
            uiController.Init(player);

            //For now: listen for player IsAlive, but can easily be extended for several players
            player.IsAlive.Subscribe(b => PlayerAliveChanged(b));
        }

        //New CompositeDisposable
        disposables = new CompositeDisposable();

        //Reactive subsriptions
        InputController.Instance.Cancel.Subscribe(cancel => HandleCancelInput(cancel)).AddTo(disposables); //esc, pad B

        //Manually call OnSceneLoaded for the first time because it doesn't trigger when game is launched
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    //What esc (xbox pad B) does in different scenes/menus
    private void HandleCancelInput(bool cancelDown)
    {
        //When cancel button is released...
        if (!cancelDown && !ignoreFirstCancel)
        {
            Debug.Log("Cancel down, load map...");
            switch (currentSceneName)
            {
                case GameHelper.MainMenu:
                    //Quit dialog not implemented yet
                    break;
                case GameHelper.WorldMap:
                    //Pressed esc or B in world map, load menu
                    LoadScene(GameHelper.MainMenu);
                    break;
                default:
                    //Returns to map, ingame settings not implemented yet
                    LoadScene(GameHelper.WorldMap);
                    break;
            }
        }

        //NOTE: hackish, prevent first reactive trigger from telling to load scene
        ignoreFirstCancel = false;
    }

    private void PlayerAliveChanged(bool isAlive)
    {
        //Player died
        if (!isAlive && !resetting && resetables.Any())
            StartCoroutine(ResetSequence());
    }

    //Wait, then reset the whole scene
    IEnumerator ResetSequence()
    {
        resetting = true;

        yield return new WaitForSeconds(2f);
        ResetSceneObjects();

        resetting = false;
    }

    //Call reset on all scene Platformer objects
    private void ResetSceneObjects()
    {
        resetables.ForEach(obj => obj.Reset());
    }

    private void GetSceneResetables()
    {
        //Get all PlatformerObjects (objects with Reset() method)
        resetables = FindObjectsOfType<PlatformerObject>().ToList();
    }

    private UiController InstantiateUiPrefab()
    {
        GameObject uiGo = Instantiate(Resources.Load("UI")) as GameObject;
        return uiGo.GetComponent<UiController>();
    }

    private PlayerController InstantiatePlayer()
    {
        GameObject playerGo = Instantiate(Resources.Load("Player")) as GameObject;        
        return playerGo.GetComponent<PlayerController>();
    }

    private PlayerSpawnPoint GetPlayerSpawnPoint()
    {
        //Get all spawn points...
        List<PlayerSpawnPoint> spawnPointsInScene = FindObjectsOfType<PlayerSpawnPoint>().ToList();
        //..but for now, just use first and notify if there are none or more
        if (!spawnPointsInScene.Any())
            Debug.LogError("No player spawn points found! Add spawn point to scene.");
        if (spawnPointsInScene.Count > 1)
            Debug.LogError("More than one spawn point in scene, selecting first found. Is this intentional?");

        return spawnPointsInScene.FirstOrDefault();
    }
}
