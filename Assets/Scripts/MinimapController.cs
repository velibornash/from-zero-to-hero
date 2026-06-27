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
    GameObject minimapHud;  // Persistent parent for all minimap UI — immune to popup moves
    RectTransform mapRect;
    List<Image> enemyMarkers = new List<Image>();
    List<Image> slotMarkers = new List<Image>();
    Image playerMarker;

    // World bounds for mapping to minimap
    float worldMinX = -100f, worldMaxX = 100f;
    float worldMinZ = -100f, worldMaxZ = 100f;

    void Awake()
    {
        // Destroy ALL stale minimap GameObjects that may have leaked from a previous session
        foreach (var name in new[] { "MinimapCamera", "MinimapHUD", "Minimap" })
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);
        }
    }

    void Start()
    {
        enemyMarkers.Clear();
        slotMarkers.Clear();

        if (target == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            if (player != null) target = player.transform;
        }

        // Create minimap camera (separate from UI)
        var camGo = new GameObject("MinimapCamera");
        minimapCam = camGo.AddComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = mapSize * 0.5f;
        minimapCam.transform.position = new Vector3(0, height, 0);
        minimapCam.transform.rotation = Quaternion.Euler(90, 0, 0);
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = new Color(0.15f, 0.2f, 0.1f);
        minimapCam.depth = -1;
        minimapCam.useOcclusionCulling = false;

        rt = new RenderTexture(texSize, texSize, 24);
        rt.Create();
        minimapCam.targetTexture = rt;

        // Find the canvas
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("MinimapController: no Canvas found!");
            return;
        }

        // Create a dedicated parent for all minimap UI
        // This is CRITICAL — the parent must NOT be the popup's overlay
        // so that building popup movements don't drag the minimap with them
        minimapHud = new GameObject("MinimapHUD");
        var hudRt = minimapHud.AddComponent<RectTransform>();
        hudRt.SetParent(canvas.transform, false);
        hudRt.anchorMin = new Vector2(1, 0);
        hudRt.anchorMax = new Vector2(1, 0);
        hudRt.pivot = new Vector2(1, 0);
        hudRt.anchoredPosition = new Vector2(-16, 16);
        hudRt.sizeDelta = new Vector2(texSize + 12, texSize + 12);
        hudRt.localScale = Vector3.one;

        // Raw image inside the HUD parent
        var rawGo = new GameObject("MinimapImage");
        rawGo.transform.SetParent(minimapHud.transform, false);
        mapRect = rawGo.AddComponent<RectTransform>();
        mapRect.anchorMin = Vector2.zero;
        mapRect.anchorMax = Vector2.one;
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;
        var rawImg = rawGo.AddComponent<RawImage>();
        rawImg.texture = rt;
        rawImg.color = new Color(1, 1, 1, 0.65f);
        rawImg.raycastTarget = false;

        // Gold border inside the HUD parent
        var border = new GameObject("MinimapBorder");
        border.transform.SetParent(minimapHud.transform, false);
        var bRt = border.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.offsetMin = new Vector2(-6, -6);
        bRt.offsetMax = new Vector2(6, 6);
        var bImg = border.AddComponent<Image>();
        bImg.sprite = UIStyleHelper.Make9SliceBorder(96, 96, 14, 18);
        bImg.type = Image.Type.Sliced;
        bImg.color = Color.white;
        bImg.raycastTarget = false;

        // Player marker (anchored to UV in LateUpdate)
        playerMarker = MakeMarkerOnHud("PlayerMarker", 14f, new Color(0.2f, 0.9f, 0.2f, 0.95f));
    }

    void ForceMinimapPosition()
    {
        if (minimapHud == null) return;
        var hudRt = minimapHud.GetComponent<RectTransform>();
        if (hudRt == null) return;
        // Force the HUD parent to stay in the bottom-right corner
        hudRt.anchorMin = new Vector2(1, 0);
        hudRt.anchorMax = new Vector2(1, 0);
        hudRt.pivot = new Vector2(1, 0);
        hudRt.anchoredPosition = new Vector2(-16, 16);
        hudRt.sizeDelta = new Vector2(texSize + 12, texSize + 12);
    }

    Image MakeMarkerOnHud(string name, float size, Color color)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(minimapHud.transform, false);
        var rt = marker.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
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
        // Force the minimap HUD to stay in the bottom-right corner EVERY frame
        // so that popup panel movements don't drag it with them
        ForceMinimapPosition();

        if (minimapHud == null || minimapCam == null) return;

        // Update player marker
        if (target != null && playerMarker != null)
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
            var m = MakeMarkerOnHud("EnemyMarker", 8f, new Color(0.95f, 0.15f, 0.15f, 0.9f));
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
            var m = MakeMarkerOnHud("SlotMarker", 10f, new Color(0.8f, 0.7f, 0.1f, 0.6f));
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
