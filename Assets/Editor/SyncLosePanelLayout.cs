using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SyncLosePanelLayout
{
    // Values from Level 3 (User's source)
    static readonly Vector2 loseImagePos = new Vector2(0f, 100f);
    static readonly Vector2 loseImageSize = new Vector2(250f, 250f);
    
    static readonly Vector2 retryBtnPos = new Vector2(-10f, -25f);
    static readonly Vector2 retryBtnSize = new Vector2(200f, 300f);

    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel to All Levels")]
    [MenuItem("Tools/Sync Lose Panel 2-25")] public static void Run25() { SyncRange(4, 100); }
    [MenuItem("Tools/Sync Lose Panel 26-50")] public static void Run50() { SyncRange(26, 50); }
    [MenuItem("Tools/Sync Lose Panel 51-75")] public static void Run75() { SyncRange(51, 75); }
    [MenuItem("Tools/Sync Lose Panel 76-100")] public static void Run100() { SyncRange(76, 100); }

    public static void SyncRange(int start, int end)
    {
        Sprite loseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/you lose.png");
        Sprite retrySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/retry.png");

        for (int i = start; i <= end; i++)
        {
            string scenePath = "Assets/Scenes/Level" + i + ".unity";
            var scene = EditorSceneManager.OpenScene(scenePath);
            if (!scene.IsValid()) continue;

            GameFlowManager flow = Object.FindAnyObjectByType<GameFlowManager>(FindObjectsInactive.Include);
            
            GameObject losePanel = null;
            Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in all) if (t.name == "LosePanel") { losePanel = t.gameObject; break; }

            if (losePanel == null) continue;
            if (flow != null) { flow.losePanel = losePanel; EditorUtility.SetDirty(flow); }

            Transform oldImg = losePanel.transform.Find("Lose image") ?? losePanel.transform.Find("LoseText");
            if (oldImg != null) { Object.DestroyImmediate(oldImg.gameObject); }
            
            GameObject newImg = new GameObject("Lose image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            newImg.transform.SetParent(losePanel.transform, false);
            Image img = newImg.GetComponent<Image>();
            img.sprite = loseSprite; img.preserveAspect = true;
            RectTransform rt = newImg.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = loseImagePos; rt.sizeDelta = loseImageSize;

            Transform btnT = losePanel.transform.Find("RetryButton");
            if (btnT == null) { foreach(Transform c in losePanel.transform) if(c.GetComponent<Button>() != null) { btnT = c; break; } }
            
            if (btnT != null)
            {
                btnT.name = "RetryButton";
                Image bImg = btnT.GetComponent<Image>() ?? btnT.gameObject.AddComponent<Image>();
                bImg.sprite = retrySprite; bImg.preserveAspect = true;
                RectTransform brt = btnT.GetComponent<RectTransform>();
                brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
                brt.anchoredPosition = retryBtnPos; brt.sizeDelta = retryBtnSize;

                Button btn = btnT.GetComponent<Button>();
                if (btn != null && flow != null)
                {
                    while (btn.onClick.GetPersistentEventCount() > 0)
                        UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, flow.OnRetryPressed);
                    EditorUtility.SetDirty(btn);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("Finished " + start + "-" + end);
    }

    static Image imgImgComp(GameObject g) {
        Image i = g.GetComponent<Image>();
        if (i == null) i = g.AddComponent<Image>();
        return i;
    }
}
