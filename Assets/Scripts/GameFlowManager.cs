using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject losePanel;
    public GameObject winPanel;

    private static bool hasGameEverStarted = false;

    void Start()
    {
        bool isLevelOne = SceneManager.GetActiveScene().buildIndex == 0;

        // Only pause and show the start screen on Level 1, the very first time.
        // For ALL other levels, always unpause and hide the start panel.
        if (isLevelOne && !hasGameEverStarted)
        {
            Time.timeScale = 0f;
            if (startPanel != null)
                startPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (startPanel != null)
                startPanel.SetActive(false);
        }

        // Always hide win/lose panels when a new level starts
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void OnPlayPressed()
    {
        hasGameEverStarted = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OnRetryPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowLoseScreen()
    {
        Time.timeScale = 0f;

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void ShowWinScreen()
    {
        Time.timeScale = 0f;

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void OnNextPressed()
    {
        Time.timeScale = 1f;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (currentIndex + 1 < totalScenes)
        {
            SceneManager.LoadScene(currentIndex + 1);
        }
        else
        {
            Debug.Log("No more levels");
        }
    }
}