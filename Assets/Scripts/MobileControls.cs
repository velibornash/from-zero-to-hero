using UnityEngine;
using UnityEngine.UI;

public class MobileControls : MonoBehaviour
{
    void Start()
    {
        if (IsMobile())
            BuildJoystick();
    }

    bool IsMobile()
    {
        return Application.isMobilePlatform || Input.touchSupported;
    }

    void BuildJoystick()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        // Joystick base — dark parchment disk
        var bg = new GameObject("Joystick");
        bg.transform.SetParent(transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0);
        bgRt.anchorMax = new Vector2(0, 0);
        bgRt.pivot = new Vector2(0.5f, 0.5f);
        bgRt.anchoredPosition = new Vector2(180, 180);
        bgRt.sizeDelta = new Vector2(180, 180);
        var bgImg = bg.AddComponent<Image>();
        bgImg.sprite = MakeDiskSprite(180, 0.5f, 0.4f, 0.2f, 0.55f);
        bgImg.color = Color.white;
        bgImg.raycastTarget = true;

        // Gold ring around the joystick
        var ring = new GameObject("Ring");
        ring.transform.SetParent(bg.transform, false);
        var rRt = ring.AddComponent<RectTransform>();
        rRt.anchorMin = Vector2.zero;
        rRt.anchorMax = Vector2.one;
        rRt.offsetMin = new Vector2(-4, -4);
        rRt.offsetMax = new Vector2(4, 4);
        var rImg = ring.AddComponent<Image>();
        rImg.sprite = MakeRingSprite(180, 0.9f, 0.65f, 0.25f, 0.95f);
        rImg.color = Color.white;
        rImg.raycastTarget = false;

        // Joystick handle — gold filled disk
        var handle = new GameObject("Handle");
        handle.transform.SetParent(bg.transform, false);
        var hRt = handle.AddComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0.5f, 0.5f);
        hRt.anchorMax = new Vector2(0.5f, 0.5f);
        hRt.pivot = new Vector2(0.5f, 0.5f);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta = new Vector2(82, 82);
        var hImg = handle.AddComponent<Image>();
        hImg.sprite = MakeDiskSprite(82, 0.92f, 0.72f, 0.28f, 1f);
        hImg.color = Color.white;
        hImg.raycastTarget = false;

        // Handle inner — darker bronze
        var inner = new GameObject("Inner");
        inner.transform.SetParent(handle.transform, false);
        var iRt = inner.AddComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero;
        iRt.anchorMax = Vector2.one;
        iRt.offsetMin = new Vector2(12, 12);
        iRt.offsetMax = new Vector2(-12, -12);
        var iImg = inner.AddComponent<Image>();
        iImg.sprite = MakeDiskSprite(58, 0.55f, 0.35f, 0.10f, 1f);
        iImg.color = Color.white;
        iImg.raycastTarget = false;

        // Center dot
        var center = new GameObject("Center");
        center.transform.SetParent(bg.transform, false);
        var cRt = center.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0.5f);
        cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.pivot = new Vector2(0.5f, 0.5f);
        cRt.anchoredPosition = Vector2.zero;
        cRt.sizeDelta = new Vector2(10, 10);
        var cImg = center.AddComponent<Image>();
        cImg.sprite = MakeDiskSprite(10, 0.95f, 0.78f, 0.30f, 1f);
        cImg.color = Color.white;
        cImg.raycastTarget = false;

        bg.AddComponent<Joystick>();
    }

    Sprite MakeDiskSprite(int size, float r, float g, float b, float a)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        float c = size * 0.5f;
        float rad = c - 1;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                cols[y * size + x] = d <= rad ? new Color(r, g, b, a) : new Color(0, 0, 0, 0);
            }
        tex.SetPixels(cols);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    Sprite MakeRingSprite(int size, float r, float g, float b, float a)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        float c = size * 0.5f;
        float rOut = c - 1;
        float rIn = c - 7;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                if (d <= rOut && d >= rIn)
                    cols[y * size + x] = new Color(r, g, b, a);
                else
                    cols[y * size + x] = new Color(0, 0, 0, 0);
            }
        tex.SetPixels(cols);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
