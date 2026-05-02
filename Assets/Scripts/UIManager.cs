using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI chickText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI resultText;
    public Image dimImage;
    public Image flashImage;

    [Header("State")]
    private bool isGameOver = false;
    private bool isWin = false;

    [Header("New Panels (Optional)")]
    public GameObject startPanel;
    public GameObject losePanel;
    public GameObject winPanel;

    private static bool gameHasStartedBefore = false;

    void Start()
    {
        // Pause the game on Level 1 until they click Play
        if (SceneManager.GetActiveScene().buildIndex == 0 && !gameHasStartedBefore)
        {
            Time.timeScale = 0f; 
        }
        else
        {
            Time.timeScale = 1f;
        }

        // 1. Fix EventSystem missing (prevents button clicks)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. Fix Start Panel appearing on every retry
        // Try to auto-find it if it wasn't assigned in the inspector
        if (startPanel == null) startPanel = FindInactivePanel("StartPanel", "Start Panel", "start panel");

        if (startPanel != null)
        {
            if (gameHasStartedBefore)
            {
                startPanel.SetActive(false);
            }
            else
            {
                gameHasStartedBefore = true; // Mark as started so it hides next time
            }
        }

        // Auto-find and hide Win/Lose panels at start just in case they were left on in the editor
        if (winPanel == null) winPanel = FindInactivePanel("WinPanel", "Win Panel", "win panel");
        if (losePanel == null) losePanel = FindInactivePanel("LosePanel", "Lose Panel", "lose panel");

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        if (resultText != null)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }

        // 3. Fix Raycast Blocking (dimImage covers buttons and blocks clicks)
        if (dimImage != null) 
        {
            dimImage.color = new Color(0, 0, 0, 0);
            dimImage.raycastTarget = false; // Allows clicks to pass through to your Retry button!
        }
        
        if (flashImage != null) 
        {
            flashImage.color = new Color(1, 0, 0, 0);
            flashImage.raycastTarget = false;
        }
    }

    // Call this from your Retry Button's OnClick event!
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Call this from your Play Button's OnClick event!
    public void StartGame()
    {
        Time.timeScale = 1f; // Unpause the game!
        gameHasStartedBefore = true;

        if (startPanel != null) startPanel.SetActive(false);

        // Hide the start image (case-insensitive search just in case)
        foreach (Transform t in FindObjectsOfType<Transform>(true))
        {
            if (t.name.ToLower() == "start image" || t.name.ToLower() == "start image 1")
            {
                t.gameObject.SetActive(false);
            }
            if (t.name.ToLower() == "start button" || t.name.ToLower() == "button")
            {
                t.gameObject.SetActive(false);
            }
        }

        // We can just hide the button itself since this method is called by the button
        UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.SetActive(false);
    }

    // Call this from your Win Panel's "Next Level" button!
    public void NextLevel()
    {
        Time.timeScale = 1f;
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0; // Loop back to the first level if we beat the last one
        }
        SceneManager.LoadScene(nextScene);
    }

    void Update()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        int collected = player.chicks.Count;
        int goal = player.targetChicks;

        if (chickText != null)
        {
            chickText.text = "Chicks: " + collected + " / " + goal;

            if (collected < goal)
                chickText.color = Color.red;
            else if (collected == goal)
                chickText.color = Color.yellow;
            else
                chickText.color = Color.white;
        }

        if (levelText != null)
        {
            levelText.text = "LEVEL " + LevelGenerator.currentLevel;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (isWin && Input.GetKeyDown(KeyCode.Space))
        {
            NextLevel();
        }
    }

    public void ShowWin()
    {
        if (isGameOver) return;
        isGameOver = true;
        isWin = true;

        if (winPanel != null) winPanel.SetActive(true); // Automatically show your new Win Panel!

        Time.timeScale = 0f;

        if (CameraShake.Instance != null) CameraShake.Instance.StopShake();
    }

    public void ShowFail()
    {
        if (isGameOver) return;
        isGameOver = true;

        StopAllCoroutines();
        StartCoroutine(FailSequence());
    }

    private IEnumerator FailSequence()
    {
        if (CameraShake.Instance != null) CameraShake.Instance.StopShake();

        if (losePanel != null) losePanel.SetActive(true); // Automatically show your Lose Panel!

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(0.05f);

        if (dimImage != null)
        {
            dimImage.transform.SetAsLastSibling();

            Color c = dimImage.color;
            c.a = 0f;
            dimImage.color = c;

            float duration = 0.4f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, 0.5f, t / duration);
                c.a = alpha;
                dimImage.color = c;
                yield return null;
            }

            c.a = 0.5f;
            dimImage.color = c;
        }
    }

    public void TriggerDamageFlash()
    {
        if (flashImage != null)
        {
            StopCoroutine("FlashRoutine");
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1, 0, 0, 0.4f);
        float elapsed = 0;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            flashImage.color = Color.Lerp(
                new Color(1, 0, 0, 0.4f),
                new Color(1, 0, 0, 0),
                elapsed / duration
            );
            yield return null;
        }

        flashImage.color = new Color(1, 0, 0, 0);
    }

    private GameObject FindInactivePanel(params string[] names)
    {
        // Searches all objects in the entire scene (even inactive ones) for the panels
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            // Skip internal Unity objects and prefabs not in the scene
            if (go.hideFlags != HideFlags.None || go.scene.buildIndex == -1) continue; 
            
            foreach (string searchName in names)
            {
                if (go.name.Equals(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return go;
                }
            }
        }
        return null;
    }
}