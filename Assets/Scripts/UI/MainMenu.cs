using UnityEngine;
using UnityEngine.SceneManagement;
using Survivor.Core;

public class MainMenu : MonoBehaviour
{
    public GameObject savesPanel;
    public GameObject preferencePanel;

    [Header("Title Scene Audio")]
    public AudioSource ambientSource;

    [Header("Title Text Animation")]
    public float speed;
    public float frequency;
    public float amplitude;
    public GameObject titlePanel;
    private float t = 0;

    private void Update()
    {
        t += Time.deltaTime * speed;
        for (int i = 0; i < titlePanel.transform.childCount; i++)
        {
            Vector3 pos = titlePanel.transform.GetChild(i).position;
            titlePanel.transform.GetChild(i).position = pos + new Vector3(0, (Mathf.Sin((t + i) * frequency) * amplitude), 0);
        }
    }

    public void StartGameButton()
    {
        UIPromptManager.Instance.PermissionPrompt("Start a new game?", ()=> { SceneManager.LoadScene("Game"); });
    }

    public void SavesButton()
    {
        savesPanel.SetActive(true);
    }

    public void SettingsButton()
    {
        preferencePanel.SetActive(true);
    }

    public void BackButton()
    {
        savesPanel.SetActive(false);
        preferencePanel.SetActive(false);
    }

    public void QuitGameButton()
    {
        UIPromptManager.Instance.PermissionPrompt("Quit game?", Application.Quit);
    }

    public void StartSavedGame(int gameIndex)
    {
        if (System.IO.Directory.Exists(Application.persistentDataPath + $"/game{gameIndex}"))
        {
            GameManager.GameIndex = gameIndex;
        }

        StartGameButton();
    }
}
