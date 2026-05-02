using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SpriteExtractor : EditorWindow
{
    [MenuItem("Tools/Extract Chicks Sprites")]
    public static void Extract()
    {
        string[] guids = AssetDatabase.FindAssets("Chickscombined t:Texture2D");
        if (guids.Length == 0) 
        {
            Debug.LogError("Could not find Chickscombined texture in AssetDatabase.");
            return;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) 
        {
            Debug.LogError("Texture not found");
            return;
        }

        int width = tex.width;
        int height = tex.height;
        Color[] pixels = tex.GetPixels();
        bool[,] mask = new bool[width, height];

        // Background color reference (top left pixel)
        Color bgRef = pixels[height * width - width]; 

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                Color c = pixels[i];
                
                float dist = Vector3.Distance(new Vector3(c.r, c.g, c.b), new Vector3(bgRef.r, bgRef.g, bgRef.b));
                
                if (dist < 0.25f)
                {
                    pixels[i] = new Color(0, 0, 0, 0);
                    mask[x, y] = false;
                }
                else
                {
                    float maxRB = Mathf.Max(c.r, c.b);
                    if (c.g > maxRB + 0.1f && dist < 0.5f) {
                        float despillFactor = (c.g - maxRB) * 2f;
                        c.a = Mathf.Clamp01(1.0f - despillFactor);
                        c.g = maxRB;
                        pixels[i] = c;
                    }
                    mask[x, y] = true;
                }
            }
        }

        List<RectInt> islands = new List<RectInt>();
        bool[,] visited = new bool[width, height];

        int[] dx = {-1, 1, 0, 0, -1, 1, -1, 1};
        int[] dy = {0, 0, -1, 1, -1, -1, 1, 1};

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mask[x, y] && !visited[x, y])
                {
                    int minX = x, maxX = x, minY = y, maxY = y;
                    Queue<Vector2Int> q = new Queue<Vector2Int>();
                    q.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;
                    int pixelCount = 0;

                    while (q.Count > 0)
                    {
                        Vector2Int curr = q.Dequeue();
                        pixelCount++;
                        
                        if (curr.x < minX) minX = curr.x;
                        if (curr.x > maxX) maxX = curr.x;
                        if (curr.y < minY) minY = curr.y;
                        if (curr.y > maxY) maxY = curr.y;

                        for (int j = 0; j < 8; j++)
                        {
                            int nx = curr.x + dx[j];
                            int ny = curr.y + dy[j];
                            
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (mask[nx, ny] && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    q.Enqueue(new Vector2Int(nx, ny));
                                }
                            }
                        }
                    }
                    
                    if (pixelCount > 200) 
                    {
                        minX = Mathf.Max(0, minX - 5);
                        minY = Mathf.Max(0, minY - 5);
                        maxX = Mathf.Min(width - 1, maxX + 5);
                        maxY = Mathf.Min(height - 1, maxY + 5);
                        
                        islands.Add(new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1));
                    }
                }
            }
        }

        islands.Sort((a, b) => {
            int aY = -a.y;
            int bY = -b.y;
            if (Mathf.Abs(aY - bY) > 50) return aY.CompareTo(bY);
            return a.x.CompareTo(b.x);
        });

        Debug.Log($"Found {islands.Count} objects to extract.");

        string[] names = { "Chick_1", "Chick_2", "Chick_3", "Chick_4", "Nest" };

        for (int i = 0; i < islands.Count; i++)
        {
            RectInt r = islands[i];
            Color[] slice = new Color[r.width * r.height];
            for (int sy = 0; sy < r.height; sy++)
            {
                for (int sx = 0; sx < r.width; sx++)
                {
                    slice[sy * r.width + sx] = pixels[(r.y + sy) * width + (r.x + sx)];
                }
            }
            
            Texture2D newTex = new Texture2D(r.width, r.height, TextureFormat.RGBA32, false);
            newTex.SetPixels(slice);
            newTex.Apply();
            
            byte[] bytes = newTex.EncodeToPNG();
            string name = i < names.Length ? names[i] : $"Extracted_{i+1}";
            string newPath = $"Assets/Sprites/{name}.png";
            File.WriteAllBytes(newPath, bytes);
            Debug.Log($"Saved: {newPath}");
            GameObject.DestroyImmediate(newTex);
        }

        AssetDatabase.Refresh();

        foreach(string n in names) {
            string p = $"Assets/Sprites/{n}.png";
            TextureImporter timporter = AssetImporter.GetAtPath(p) as TextureImporter;
            if(timporter != null) {
                timporter.textureType = TextureImporterType.Sprite;
                timporter.spriteImportMode = SpriteImportMode.Single;
                timporter.alphaIsTransparency = true;
                timporter.SaveAndReimport();
            }
        }
        
        Debug.Log("Extraction complete!");
    }
}
