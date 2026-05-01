using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public enum FloorType { Safe, Reward, Risk, Empty }

    [System.Serializable]
    public class LevelConfig
    {
        public int floors;
        public int cats;
        public int totalChicks;
        public int targetChicks;
        public string gapPattern;
    }

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject chickPrefab;
    public GameObject catPrefab;
    public GameObject exitPrefab;

    [Header("Game Settings")]
    public static int currentLevel = 1;
    public float floorSpacing = 5.0f;
    public float gapWidth = 3.5f;

    public static float screenLimit;
    private int chicksSpawned = 0;
    private int catsSpawned   = 0;
    private int lastGapPattern = -1;

    private GameObject firstFloorLeft;
    private GameObject firstFloorRight;
    private GameObject secondLastLeft;
    private GameObject secondLastRight;

    private List<GameObject> floorPlatforms = new List<GameObject>();

    private Dictionary<int, LevelConfig> levelConfigs = new Dictionary<int, LevelConfig>()
    {
        { 1,  new LevelConfig { floors = 3, cats = 0, totalChicks = 4,  targetChicks = 2, gapPattern = "center" } },
        { 2,  new LevelConfig { floors = 4, cats = 1, totalChicks = 5,  targetChicks = 3, gapPattern = "mixed" } },
        { 3,  new LevelConfig { floors = 5, cats = 1, totalChicks = 6,  targetChicks = 4, gapPattern = "mixed" } },
        { 4,  new LevelConfig { floors = 5, cats = 2, totalChicks = 6,  targetChicks = 4, gapPattern = "alternating" } },
        { 5,  new LevelConfig { floors = 6, cats = 2, totalChicks = 7,  targetChicks = 5, gapPattern = "random" } },
        { 6,  new LevelConfig { floors = 6, cats = 3, totalChicks = 8,  targetChicks = 5, gapPattern = "random" } },
        { 7,  new LevelConfig { floors = 7, cats = 3, totalChicks = 9,  targetChicks = 6, gapPattern = "mixed" } },
        { 8,  new LevelConfig { floors = 7, cats = 4, totalChicks = 9,  targetChicks = 6, gapPattern = "mixed" } },
        { 9,  new LevelConfig { floors = 8, cats = 4, totalChicks = 10, targetChicks = 7, gapPattern = "random" } },
        { 10, new LevelConfig { floors = 8, cats = 5, totalChicks = 10, targetChicks = 7, gapPattern = "random" } }
    };

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    void Start()
    {
        UnityEngine.Random.InitState(currentLevel * 1000);
        if (Camera.main == null) return;

        screenLimit = Camera.main.orthographicSize * 0.5625f;
        GenerateLevel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            currentLevel++;
            if (currentLevel > 10) currentLevel = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    // =========================================================
    // LEVEL GENERATION
    // =========================================================

    void GenerateLevel()
    {
        chicksSpawned  = 0;
        catsSpawned    = 0;
        lastGapPattern = -1;
        floorPlatforms.Clear();

        firstFloorLeft  = null;
        firstFloorRight = null;
        secondLastLeft  = null;
        secondLastRight = null;

        LevelConfig config = GetConfig(currentLevel);

        for (int i = 0; i < config.floors; i++)
        {
            float yPos = -i * floorSpacing;
            if (i == config.floors - 1)
                CreateFinalFloor(yPos);
            else
                CreateNormalFloor(i, yPos, config);
        }

        if (chicksSpawned < config.targetChicks)
            GuaranteeChicks(config);

        // Clamp camera
        float lastFloorY    = -(config.floors - 1) * floorSpacing;
        float correctedMinY = lastFloorY + Camera.main.orthographicSize - 1.5f;
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null) cam.minYLimit = correctedMinY;

        StartCoroutine(SetupPlayer(config));
    }

    // =========================================================
    // CONFIG LOOKUP
    // =========================================================

    LevelConfig GetConfig(int level)
    {
        return levelConfigs[Mathf.Clamp(level, 1, 10)];
    }

    // =========================================================
    // FLOOR SIZING HELPER
    // =========================================================
    //
    // Floor sprite: 1249 px wide, Pixels Per Unit = 300
    //   native world width at localScale.x = 1  =>  1249 / 300 = 4.163 units
    //
    // BoxCollider2D default size = {x:1, y:1} LOCAL.
    // If we set  localScale.x = targetWidth / 4.163,
    // the collider world-width becomes (targetWidth/4.163) x 1 = tiny!
    //
    // Fix: also set  collider.size.x = 4.163  so that
    //   world collider width = localScale.x * 4.163 = targetWidth  (correct!)
    //
    private const float FLOOR_SPRITE_W = 4.163f;   // 1249 px / 300 PPU

    void ApplyFloorSize(GameObject floor, float targetWorldWidth)
    {
        targetWorldWidth = Mathf.Max(targetWorldWidth, 0.3f);

        float scaleX = targetWorldWidth / FLOOR_SPRITE_W;
        floor.transform.localScale = new Vector3(scaleX, floor.transform.localScale.y, 1f);

        // Fix collider so its world-width matches the visual width
        BoxCollider2D bc = floor.GetComponent<BoxCollider2D>();
        if (bc != null)
            bc.size = new Vector2(FLOOR_SPRITE_W, bc.size.y);
    }

    // =========================================================
    // FLOOR CREATION
    // =========================================================

    void CreateFinalFloor(float yPos)
    {
        float floorWidth = screenLimit * 2f;   // screenLimit = orthoSize * 0.5625 (portrait 9:16)

        GameObject floor = Instantiate(floorPrefab, new Vector3(0f, yPos, 0f), Quaternion.identity);
        ApplyFloorSize(floor, floorWidth);
        floorPlatforms.Add(floor);

        if (exitPrefab != null)
            Instantiate(exitPrefab, new Vector3(0f, yPos + 1f, 0f), Quaternion.identity);
    }

    void CreateNormalFloor(int floorIndex, float yPos, LevelConfig config)
    {
        float halfScreen = screenLimit;         // screenLimit = orthoSize * 0.5625 (portrait 9:16)
        float halfGap    = gapWidth * 0.5f;

        float gapX = GetGapX(floorIndex, config.gapPattern, halfScreen);

        // Left platform: from -halfScreen to (gapX - halfGap)
        float leftWidth  = halfScreen + gapX - halfGap;
        float leftCenter = (-halfScreen + gapX - halfGap) * 0.5f;

        // Right platform: from (gapX + halfGap) to halfScreen
        float rightWidth  = halfScreen - gapX - halfGap;
        float rightCenter = (gapX + halfGap + halfScreen) * 0.5f;

        leftWidth  = Mathf.Max(leftWidth,  0.3f);
        rightWidth = Mathf.Max(rightWidth, 0.3f);

        GameObject leftFloor  = Instantiate(floorPrefab, new Vector3(leftCenter,  yPos, 0f), Quaternion.identity);
        GameObject rightFloor = Instantiate(floorPrefab, new Vector3(rightCenter, yPos, 0f), Quaternion.identity);

        ApplyFloorSize(leftFloor,  leftWidth);
        ApplyFloorSize(rightFloor, rightWidth);

        floorPlatforms.Add(leftFloor);
        floorPlatforms.Add(rightFloor);

        if (floorIndex == 0)
        {
            firstFloorLeft  = leftFloor;
            firstFloorRight = rightFloor;
        }
        if (floorIndex == config.floors - 2)
        {
            secondLastLeft  = leftFloor;
            secondLastRight = rightFloor;
        }

        // Spawn chicks
        int chicksOnFloor = 0;
        if (config.totalChicks > 0 && chicksSpawned < config.totalChicks)
        {
            chicksOnFloor = Random.Range(1, 3);
            chicksOnFloor = Mathf.Min(chicksOnFloor, config.totalChicks - chicksSpawned);
        }

        for (int c = 0; c < chicksOnFloor && chickPrefab != null; c++)
        {
            bool  spawnLeft  = Random.value > 0.5f;
            float halfW      = spawnLeft ? leftWidth * 0.5f  : rightWidth * 0.5f;
            float centX      = spawnLeft ? leftCenter         : rightCenter;
            float spawnX     = centX + Random.Range(-halfW * 0.6f, halfW * 0.6f);
            Instantiate(chickPrefab, new Vector3(spawnX, yPos + 1f, 0f), Quaternion.identity);
            chicksSpawned++;
        }

        // Spawn cats (skip first floor)
        if (floorIndex > 0 && catsSpawned < config.cats && catPrefab != null)
        {
            if (Random.value > 0.4f)
            {
                bool  catLeft  = Random.value > 0.5f;
                float halfW    = catLeft ? leftWidth * 0.5f  : rightWidth * 0.5f;
                float centX    = catLeft ? leftCenter          : rightCenter;
                float catX     = centX + Random.Range(-halfW * 0.4f, halfW * 0.4f);
                Instantiate(catPrefab, new Vector3(catX, yPos + 1f, 0f), Quaternion.identity);
                catsSpawned++;
            }
        }
    }

    // =========================================================
    // GAP POSITIONING
    // =========================================================

    float GetGapX(int floorIndex, string pattern, float halfScreen)
    {
        float maxOffset = halfScreen * 0.5f;

        switch (pattern)
        {
            case "center":
                return 0f;

            case "alternating":
                if (lastGapPattern == -1) lastGapPattern = 0;
                lastGapPattern = 1 - lastGapPattern;
                return lastGapPattern == 0 ? -maxOffset * 0.5f : maxOffset * 0.5f;

            case "mixed":
                int roll = Random.Range(0, 3);
                if (roll == 0) return 0f;
                if (roll == 1) return -maxOffset * 0.6f;
                return maxOffset * 0.6f;

            case "random":
                return Random.Range(-maxOffset, maxOffset);

            default:
                return 0f;
        }
    }

    // =========================================================
    // GUARANTEE MINIMUM CHICKS
    // =========================================================

    void GuaranteeChicks(LevelConfig config)
    {
        if (chickPrefab == null) return;

        GameObject anchor = secondLastLeft != null ? secondLastLeft :
                            firstFloorLeft  != null ? firstFloorLeft : null;
        if (anchor == null) return;

        // Recover actual world half-width from the collider
        BoxCollider2D bc = anchor.GetComponent<BoxCollider2D>();
        float worldHalfW = bc != null
            ? anchor.transform.localScale.x * bc.size.x * 0.5f
            : 1f;

        int needed = config.targetChicks - chicksSpawned;
        for (int i = 0; i < needed; i++)
        {
            float spawnX = anchor.transform.position.x + Random.Range(-worldHalfW * 0.5f, worldHalfW * 0.5f);
            float spawnY = anchor.transform.position.y + 1f;
            Instantiate(chickPrefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
            chicksSpawned++;
        }
    }

    // =========================================================
    // PLAYER SETUP  (one frame delay so physics settles)
    // =========================================================

    IEnumerator SetupPlayer(LevelConfig config)
    {
        yield return null;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) yield break;

        float spawnX = 0f;
        float spawnY = 1f;

        if (firstFloorLeft != null)
        {
            // World half-width from collider
            BoxCollider2D bc = firstFloorLeft.GetComponent<BoxCollider2D>();
            float halfW = bc != null
                ? firstFloorLeft.transform.localScale.x * bc.size.x * 0.5f
                : 1f;
            spawnX = firstFloorLeft.transform.position.x + Random.Range(-halfW * 0.3f, halfW * 0.3f);
            spawnY = firstFloorLeft.transform.position.y + 1f;
        }

        player.transform.position = new Vector3(spawnX, spawnY, 0f);
        player.targetChicks        = config.targetChicks;
        player.totalChicksInLevel  = config.totalChicks;

        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null) cam.target = player.transform;
    }
}