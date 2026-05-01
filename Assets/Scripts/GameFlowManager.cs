using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject losePanel;
    public GameObject winPanel;

    void Start()
    {
        Time.timeScale = 0f;

        if (startPanel != null)
            startPanel.SetActive(true);
    }

    public void OnPlayPressed()
    {
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