using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    public Transform target;
    public float height = 140f;
    public float mapSize = 200f;
    public int texSize = 384;

    Camera minimapCam;
    RenderTexture rt;
    RawImage rawImage;

    RectTransform mapRect;
    List<Image> enemyMarkers = new List<Image>();
    List<Image> slotMarkers = new List<Image>();
    Image playerMarker;

    // World bounds for mapping to minimap
    float worldMinX = -100f, worldMaxX = 100f;
    float worldMinZ = -100f, worldMaxZ = 100f;

    void Start()
    {
        if (target == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            if (player != null) target = player.transform;
        }

        rt = new RenderTexture(texSize, texSize, 24);
        rt.Create();

        var go = new GameObject("MinimapCamera");
        minimapCam = go.AddComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = mapSize * 0.5f;
        // Static camera — shows the entire world
        minimapCam.transform.position = new Vector3(0, height, 0);
        minimapCam.transform.rotation = Quaternion.Euler(90, 0, 0);
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = new Color(0.15f, 0.2f, 0.1f);
        minimapCam.targetTexture = rt;
        minimapCam.depth = -1;

        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        rawImage = new GameObject("Minimap").AddComponent<RawImage>();
        rawImage.transform.SetParent(canvas.transform, false);
        rawImage.raycastTarget = false;

        mapRect = rawImage.GetComponent<RectTransform>();
        // Force anchor and position to bottom-right corner
        ForceMinimapPosition();

        rawImage.texture = rt;
        rawImage.color = new Color(1, 1, 1, 0.65f);

        var border = new GameObject("MinimapBorder");
        border.transform.SetParent(rawImage.transform, false);
        var bRt = border.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.sizeDelta = new Vector2(0, 0);
        var bImg = border.AddComponent<Image>();
        bImg.sprite = MakeWhiteSprite();
        bImg.color = new Color(0.25f, 0.15f, 0.07f, 0.7f);
        bImg.type = Image.Type.Simple;
        bImg.raycastTarget = false;

        playerMarker = MakeMarker("PlayerMarker", new Vector2(0.5f, 0.5f), 14f,
            new Color(0.2f, 0.9f, 0.2f, 0.95f));
    }

    void ForceMinimapPosition()
    {
        if (mapRect == null) return;
        mapRect.anchorMin = new Vector2(1, 0);
        mapRect.anchorMax = new Vector2(1, 0);
        mapRect.pivot = new Vector2(1, 0);
        mapRect.anchoredPosition = new Vector2(-16, 16);
        mapRect.sizeDelta = new Vector2(texSize + 12, texSize + 12);
    }

    Image MakeMarker(string name, Vector2 anchor, float size, Color color)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(rawImage.transform, false);
        var rt = marker.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(size, size);
        var img = marker.AddComponent<Image>();
        img.sprite = MakeCircleSprite();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Sprite MakeCircleSprite()
    {
        int s = 16;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        var cols = new Color[s * s];
        float r = s * 0.5f - 1f;
        Vector2 c = new Vector2(s * 0.5f, s * 0.5f);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                cols[y * s + x] = d <= r ? Color.white : Color.clear;
            }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
    }

    Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }

    Vector2 WorldToMinimapUV(Vector3 worldPos)
    {
        return new Vector2(
            (worldPos.x - worldMinX) / (worldMaxX - worldMinX),
            (worldPos.z - worldMinZ) / (worldMaxZ - worldMinZ)
        );
    }

    void LateUpdate()
    {
        // Force minimap to stay at bottom-right corner every frame
        ForceMinimapPosition();
        if (mapRect == null || minimapCam == null) return;

        // Update player marker
        if (target != null)
        {
            Vector2 uv = WorldToMinimapUV(target.position);
            uv.x = Mathf.Clamp01(uv.x);
            uv.y = Mathf.Clamp01(uv.y);
            playerMarker.rectTransform.anchorMin = uv;
            playerMarker.rectTransform.anchorMax = uv;
            playerMarker.rectTransform.anchoredPosition = Vector2.zero;
        }

        // Update enemy markers
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        while (enemyMarkers.Count < enemies.Length)
        {
            var m = MakeMarker("EnemyMarker", new Vector2(0.5f, 0.5f), 8f,
                new Color(0.95f, 0.15f, 0.15f, 0.9f));
            enemyMarkers.Add(m);
        }
        while (enemyMarkers.Count > enemies.Length)
        {
            Destroy(enemyMarkers[enemyMarkers.Count - 1].gameObject);
            enemyMarkers.RemoveAt(enemyMarkers.Count - 1);
        }
        for (int i = 0; i < enemies.Length; i++)
        {
            Vector2 uv = WorldToMinimapUV(enemies[i].transform.position);
            // Clamp to map bounds and dim if outside
            bool outside = uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1;
            uv.x = Mathf.Clamp01(uv.x);
            uv.y = Mathf.Clamp01(uv.y);
            var mr = enemyMarkers[i].rectTransform;
            mr.anchorMin = uv;
            mr.anchorMax = uv;
            mr.anchoredPosition = Vector2.zero;
            mr.sizeDelta = Vector2.one * 8f;
            enemyMarkers[i].color = outside
                ? new Color(0.95f, 0.15f, 0.15f, 0.35f)
                : new Color(0.95f, 0.15f, 0.15f, 0.9f);
        }

        // Update slot markers (only show unbuilt slots)
        var allSlots = FindObjectsByType<BuildSlot>(FindObjectsSortMode.None);
        var activeSlots = new List<BuildSlot>();
        foreach (var s in allSlots) if (!s.IsBuilt) activeSlots.Add(s);
        while (slotMarkers.Count < activeSlots.Count)
        {
            var m = MakeMarker("SlotMarker", new Vector2(0.5f, 0.5f), 10f,
                new Color(0.8f, 0.7f, 0.1f, 0.6f));
            slotMarkers.Add(m);
        }
        while (slotMarkers.Count > activeSlots.Count)
        {
            Destroy(slotMarkers[slotMarkers.Count - 1].gameObject);
            slotMarkers.RemoveAt(slotMarkers.Count - 1);
        }
        for (int i = 0; i < activeSlots.Count; i++)
        {
            Vector2 uv = WorldToMinimapUV(activeSlots[i].transform.position);
            uv.x = Mathf.Clamp01(uv.x);
            uv.y = Mathf.Clamp01(uv.y);
            var mr = slotMarkers[i].rectTransform;
            mr.anchorMin = uv;
            mr.anchorMax = uv;
            mr.anchoredPosition = Vector2.zero;
            mr.sizeDelta = Vector2.one * 10f;
        }
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}
