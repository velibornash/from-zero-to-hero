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
    GameObject minimapHud;
    RectTransform mapRect;
    List<Image> enemyMarkers = new List<Image>();
    List<Image> slotMarkers = new List<Image>();
    Image playerMarker;

    void Awake()
    {
        foreach (var name in new[] { "MinimapCamera", "MinimapHUD", "Minimap" })
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);
        }
    }

    void EnsureMinimap()
    {
        if (minimapCam != null && rt != null && minimapHud != null)
            return;

        if (target == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            if (player != null) target = player.transform;
        }

        Canvas canvas = null;
        var allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var c in allCanvases)
        {
            if (c.name == "Canvas" || c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c;
                break;
            }
        }
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Recreate camera if missing
        if (minimapCam == null)
        {
            minimapCam = GetComponentInChildren<Camera>();
            if (minimapCam == null)
            {
                var camGo = new GameObject("MinimapCamera");
                camGo.transform.SetParent(transform);
                minimapCam = camGo.AddComponent<Camera>();
            }
            minimapCam.orthographic = true;
            minimapCam.orthographicSize = mapSize * 0.5f;
            minimapCam.transform.SetPositionAndRotation(new Vector3(0, height, 0), Quaternion.Euler(90, 0, 0));
            minimapCam.clearFlags = CameraClearFlags.SolidColor;
            minimapCam.backgroundColor = new Color(0.15f, 0.2f, 0.1f);
            minimapCam.depth = -1;
            minimapCam.useOcclusionCulling = false;
        }

        // Recreate render texture if missing
        if (rt == null)
        {
            rt = new RenderTexture(texSize, texSize, 24);
            rt.Create();
        }
        minimapCam.targetTexture = rt;

        // Recreate HUD if missing
        if (minimapHud == null)
        {
            minimapHud = new GameObject("MinimapHUD");
            var hudRt = minimapHud.AddComponent<RectTransform>();
            hudRt.SetParent(canvas.transform, false);
            hudRt.anchorMin = new Vector2(1, 0);
            hudRt.anchorMax = new Vector2(1, 0);
            hudRt.pivot = new Vector2(1, 0);
            hudRt.anchoredPosition = new Vector2(-16, 16);
            hudRt.sizeDelta = new Vector2(texSize + 12, texSize + 12);
            hudRt.localScale = Vector3.one;

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

            playerMarker = MakeMarkerOnHud("PlayerMarker", 14f, new Color(0.2f, 0.9f, 0.2f, 0.95f));
            enemyMarkers.Clear();
            slotMarkers.Clear();
        }
    }

    void Start()
    {
        Debug.Log("MinimapController.Start()");
        EnsureMinimap();
        ForceMinimapPosition();
    }

    void ForceMinimapPosition()
    {
        if (minimapHud == null) return;
        var hudRt = minimapHud.GetComponent<RectTransform>();
        if (hudRt == null) return;
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

    Vector2 WorldToMinimapUV(Vector3 worldPos, Vector3 camPos)
    {
        float halfSize = mapSize * 0.5f;
        return new Vector2(
            (worldPos.x - (camPos.x - halfSize)) / mapSize,
            (worldPos.z - (camPos.z - halfSize)) / mapSize
        );
    }

    void LateUpdate()
    {
        EnsureMinimap();
        ForceMinimapPosition();

        if (minimapHud == null || minimapCam == null) return;

        // Camera follows the player in XZ, keeping fixed height
        if (target != null)
        {
            var pos = minimapCam.transform.position;
            pos.x = target.position.x;
            pos.z = target.position.z;
            minimapCam.transform.position = pos;
        }

        // Update player marker (always at center of minimap since camera follows player)
        if (target != null && playerMarker != null)
        {
            playerMarker.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            playerMarker.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            playerMarker.rectTransform.anchoredPosition = Vector2.zero;
        }

        // Update enemy markers
        var enemies = FindObjectsByType<Enemy>();
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
            Vector2 uv = WorldToMinimapUV(enemies[i].transform.position, minimapCam.transform.position);
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
        var allSlots = FindObjectsByType<BuildSlot>();
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
            Vector2 uv = WorldToMinimapUV(activeSlots[i].transform.position, minimapCam.transform.position);
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
