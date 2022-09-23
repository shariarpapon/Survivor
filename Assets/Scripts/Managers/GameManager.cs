using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

namespace Survivor.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static GameMode GameMode { get; private set; } = GameMode.MainMenu;
        public static int GameIndex { get; set; } = -1;
        public static bool IsGameInitialized = false;
        public static bool LoadDataFromExistingSave = false;

        public static event Action<string, float> OnComponentInitialized;
        public static event Action OnGameInitilizationComplete;
        public static event Action OnGameStarted;
        public static event Action OnGameResumed;
        public static event Action OnGamePuased;
        public static event Action OnGameStarting;

        private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
        private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

        public UserPreference userPreference;

        private void Awake() 
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }

            //Load universal data
            var globalData = FindObjectsOfType<MonoBehaviour>(true).OfType<IUniversalStorableData>();
            foreach (var gd in globalData) gd.LoadUniversalData();
        }

        private void Update() 
        {
            //Test-phase
            if (Input.GetKeyDown(KeyCode.F11)) GameUtility.SaveScreenshot();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
        {
            if (SceneManager.GetActiveScene().name == "Game") StartGame();
        }

        public void LoadScene(string scene) 
        {
            SceneManager.LoadScene(scene);
        }

        //GAME LOOP STARTS IN THIS METHOD
        public void StartGame()
        {
            if(GameIndex > -1) LoadDataFromExistingSave = true;

            Debug.Log("Starting game with index " + GameIndex);
            SetGameMode(GameMode.Starting);

            StartCoroutine(InitGame());
            StartCoroutine(AwaitInitialization());

            OnGameStarted?.Invoke();
        }

        private IEnumerator InitGame() 
        {
            var initializers = FindObjectsOfType<MonoBehaviour>(true).OfType<IInitializer>();
            ProgressManager.LogOperations(initializers.Count());

            foreach (IInitializer initializer in initializers) 
            {
                OnComponentInitialized("Initializing Component : " + initializer.ToString(), ProgressManager.Progress);

                yield return StartCoroutine(initializer.Init());
                ProgressManager.UpdateProgress();
            }
        }
    
        private IEnumerator AwaitInitialization() 
        {
            while (ProgressManager.Progress < 1) yield return null;
            OnGameInitialized();
        }

        private void OnGameInitialized() 
        {
            PlayerController.IsControllable = true;

            IsGameInitialized = true;
            GameMode = GameMode.Playing;

            OnGameInitilizationComplete?.Invoke();
            Debug.Log("<color=green>Game Initialized!</color>");
        }

        public static void PauseGame()
        {
            OnGamePuased?.Invoke();
        }

        public static void ResumeGame()
        {
            OnGameResumed?.Invoke();
        }

        public static void SetGameMode(GameMode mode) 
        {
            GameMode = mode;
            switch (GameMode) 
            {
                case GameMode.Paused:
                    OnGamePuased?.Invoke();
                    break;
                case GameMode.Playing:
                    OnGameResumed?.Invoke();
                    break;
                case GameMode.MainMenu:
                    LoadMainMenu();
                    break;
                case GameMode.Starting:
                    OnGameStarting?.Invoke();
                    break;
            }
        }

        private static void LoadMainMenu()
        {
            GameIndex = -1;
            IsGameInitialized = false;
            SceneManager.LoadScene("MainMenu");
        }

        private void OnApplicationQuit() 
        {
            var globalData = FindObjectsOfType<MonoBehaviour>(true).OfType<IUniversalStorableData>();
            foreach (var gd in globalData) gd.SaveUniversalData();
            Debug.Log("<color=green>Universal data saved...</color>");
        }
    }
}
