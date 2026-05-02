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
    public GameObject chickHUDPanel;
    public GameObject levelHUDPanel;

    private static bool gameHasStartedBefore = false;

    void Start()
    {
        bool isLevelOne = SceneManager.GetActiveScene().buildIndex == 0;

        // Only pause on Level 1 for the very first play session. Always unpause for all other levels.
        if (isLevelOne && !gameHasStartedBefore)
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

        // 2. Handle StartPanel visibility
        if (startPanel == null) startPanel = FindInactivePanel("StartPanel", "Start Panel", "start panel");

        if (startPanel != null)
        {
            // Show StartPanel ONLY on Level 1 before the game has started
            if (isLevelOne && !gameHasStartedBefore)
            {
                startPanel.SetActive(true);
                gameHasStartedBefore = true; // mark so it never shows again
            }
            else
            {
                // For every other level (Level 2+, or retrying Level 1 after death), always hide it
                startPanel.SetActive(false);
            }
        }

        // Auto-find and hide Win/Lose panels at start just in case they were left on in the editor
        if (winPanel == null) winPanel = FindInactivePanel("WinPanel", "Win Panel", "win panel");
        if (losePanel == null) losePanel = FindInactivePanel("LosePanel", "Lose Panel", "lose panel");

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Find and ensure HUD is visible at start
        if (chickHUDPanel == null) chickHUDPanel = FindInactivePanel("ChickPanel", "Chick Panel");
        if (levelHUDPanel == null) levelHUDPanel = FindInactivePanel("level Panel", "Level Panel");

        if (chickHUDPanel != null) chickHUDPanel.SetActive(!isLevelOne);
        if (levelHUDPanel != null) levelHUDPanel.SetActive(!isLevelOne);

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
        SceneManager.LoadScene(1); // Load the actual Level 1 scene
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

        // Hide HUD on Win
        if (chickHUDPanel != null) chickHUDPanel.SetActive(false);
        if (levelHUDPanel != null) levelHUDPanel.SetActive(false);

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

        // Hide HUD on Fail
        if (chickHUDPanel != null) chickHUDPanel.SetActive(false);
        if (levelHUDPanel != null) levelHUDPanel.SetActive(false);

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