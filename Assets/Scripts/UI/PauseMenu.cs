using UnityEngine;
using Survivor.Core;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    private void OnEnable() 
    {
        GameManager.OnGamePuased += OnPauseEvents;
        GameManager.OnGameResumed += OnResumeEvents;
    }

    private void OnDisable() 
    {
        GameManager.OnGamePuased -= OnPauseEvents;
        GameManager.OnGameResumed -= OnResumeEvents;
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PermissionManager.isPromptOpen == false)
        {
            if (pauseMenu.activeSelf) GameManager.SetGameMode(GameMode.Playing);
            else GameManager.SetGameMode(GameMode.Paused);
        }
    }

    public void ResumeButton() 
    {
        GameManager.SetGameMode(GameMode.Playing);
    }

    public void QuitButton() 
    {
        PermissionManager.Instance.PermissionPrompt("Quit game? Make sure to save the game first !", Application.Quit);
    }

    private void OnPauseEvents() 
    {
        PlayerController.SetCursor(CursorLockMode.None, true);
        pauseMenu.SetActive(true);
    }

    private void OnResumeEvents() 
    {
        pauseMenu.SetActive(false);
        PlayerController.SetCursor(CursorLockMode.Locked, false);
    }

    public void GoToMainMenu() 
    {
        PermissionManager.Instance.PermissionPrompt("Go to main menu? Make sure to save the game first !", ()=> GameManager.SetGameMode(GameMode.MainMenu));
    }
}
