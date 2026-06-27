using UnityEngine;
using UnityEngine.UI;

public static class UIStyleHelper
{
    public static Sprite MakeParchmentSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var cols = new Color[w * h];
        var rnd = new System.Random(42);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float v = (float)y / h;
                float noise = (float)rnd.NextDouble() * 0.08f - 0.04f;
                float edge = Mathf.Min(u, 1 - u, v, 1 - v);
                float vignette = Mathf.Clamp01(edge * 4f) * 0.25f;
                float r = 0.78f + noise - vignette;
                float g = 0.68f + noise * 0.9f - vignette * 0.9f;
                float b = 0.48f + noise * 0.7f - vignette * 0.7f;
                cols[y * w + x] = new Color(r, g, b, 1f);
            }
        tex.SetPixels(cols);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }

    public static Sprite MakeGoldBorderSprite(int w, int h, int borderWidth = 14, int cornerRadius = 18)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var cols = new Color[w * h];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float distToEdge = DistToRoundedRect(x, y, w, h, cornerRadius);
                if (distToEdge < borderWidth)
                {
                    float t = distToEdge / borderWidth;
                    Color gold = GoldGradient(t);
                    gold.a = 1f;
                    cols[y * w + x] = gold;
                }
                else if (distToEdge < borderWidth + 3)
                {
                    cols[y * w + x] = new Color(0.12f, 0.07f, 0.02f, 0.85f);
                }
            }
        tex.SetPixels(cols);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }

    public static Sprite Make9SliceBorder(int w, int h, int borderWidth = 16, int cornerRadius = 22)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var cols = new Color[w * h];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float distToEdge = DistToRoundedRect(x, y, w, h, cornerRadius);
                if (distToEdge < borderWidth)
                {
                    float t = distToEdge / borderWidth;
                    Color gold = GoldGradient(t);
                    gold.a = 1f;
                    cols[y * w + x] = gold;
                }
                if (distToEdge < 1.5f)
                    cols[y * w + x] = new Color(0.05f, 0.03f, 0.01f, 1f);
                if (distToEdge >= borderWidth && distToEdge < borderWidth + 4 && !InsideRoundedRect(x, y, w, h, cornerRadius - 2))
                    cols[y * w + x] = new Color(0.18f, 0.10f, 0.04f, 0.6f);
            }
        tex.SetPixels(cols);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        Vector4 brd = new Vector4(borderWidth, borderWidth, borderWidth, borderWidth);
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, brd);
    }

    public static Sprite MakeIconBacking(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        float c = size * 0.5f;
        float r = c - 1;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                if (d <= r)
                {
                    float t = d / r;
                    float rr = Mathf.Lerp(0.45f, 0.2f, t);
                    float gg = Mathf.Lerp(0.32f, 0.13f, t);
                    float bb = Mathf.Lerp(0.15f, 0.05f, t);
                    cols[y * size + x] = new Color(rr, gg, bb, 1f);
                }
                else
                {
                    cols[y * size + x] = new Color(0, 0, 0, 0);
                }
            }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public static Sprite MakeIconRing(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        float c = size * 0.5f;
        float rOut = c - 1;
        float rIn = c - 5;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                if (d <= rOut && d >= rIn)
                {
                    float t = (d - rIn) / (rOut - rIn);
                    float rr = Mathf.Lerp(0.85f, 0.55f, t);
                    float gg = Mathf.Lerp(0.65f, 0.35f, t);
                    float bb = Mathf.Lerp(0.20f, 0.10f, t);
                    cols[y * size + x] = new Color(rr, gg, bb, 1f);
                }
                else
                {
                    cols[y * size + x] = new Color(0, 0, 0, 0);
                }
            }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    static float DistToRoundedRect(int x, int y, int w, int h, int radius)
    {
        int cx = Mathf.Clamp(x, radius, w - 1 - radius);
        int cy = Mathf.Clamp(y, radius, h - 1 - radius);
        int dx = x - cx;
        int dy = y - cy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    static bool InsideRoundedRect(int x, int y, int w, int h, int radius)
    {
        if (x < radius && y < radius)
            return Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
        if (x >= w - radius && y < radius)
            return Vector2.Distance(new Vector2(x, y), new Vector2(w - radius, radius)) <= radius;
        if (x < radius && y >= h - radius)
            return Vector2.Distance(new Vector2(x, y), new Vector2(radius, h - radius)) <= radius;
        if (x >= w - radius && y >= h - radius)
            return Vector2.Distance(new Vector2(x, y), new Vector2(w - radius, h - radius)) <= radius;
        return x >= 0 && x < w && y >= 0 && y < h;
    }

    static Color GoldGradient(float t)
    {
        if (t < 0.4f)
            return new Color(0.95f, 0.78f, 0.30f);
        if (t < 0.7f)
            return new Color(0.85f, 0.62f, 0.20f);
        return new Color(0.55f, 0.35f, 0.10f);
    }

    public static GameObject MakeOrnatePanel(Transform parent, int width, int height)
    {
        var panel = new GameObject("OrnatePanel");
        panel.transform.SetParent(parent, false);

        var rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        var bg = panel.AddComponent<Image>();
        bg.sprite = MakeParchmentSprite(256, 256);
        bg.type = Image.Type.Sliced;
        bg.color = Color.white;
        bg.raycastTarget = true;

        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var brt = border.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        var bimg = border.AddComponent<Image>();
        bimg.sprite = Make9SliceBorder(96, 96, 14, 18);
        bimg.type = Image.Type.Sliced;
        bimg.color = Color.white;
        bimg.raycastTarget = false;

        return panel;
    }

    public static GameObject MakeOrnateIcon(Transform parent, Sprite icon, int size = 56)
    {
        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(parent, false);
        var rt = iconObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        var ring = iconObj.AddComponent<Image>();
        ring.sprite = MakeIconRing(size + 8);
        ring.color = Color.white;
        ring.raycastTarget = false;

        var inner = new GameObject("Inner");
        inner.transform.SetParent(iconObj.transform, false);
        var irt = inner.AddComponent<RectTransform>();
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(3, 3);
        irt.offsetMax = new Vector2(-3, -3);

        var innerImg = inner.AddComponent<Image>();
        innerImg.sprite = MakeIconBacking(size - 4);
        innerImg.color = new Color(0.2f, 0.15f, 0.05f, 1f);
        innerImg.raycastTarget = false;

        if (icon != null)
        {
            var sym = new GameObject("Symbol");
            sym.transform.SetParent(inner.transform, false);
            var srt = sym.AddComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(6, 6);
            srt.offsetMax = new Vector2(-6, -6);
            var sImg = sym.AddComponent<Image>();
            sImg.sprite = icon;
            sImg.color = Color.white;
            sImg.raycastTarget = false;
        }
        return iconObj;
    }

    // Create a high-visibility text label with a strong black outline + soft shadow
    // Use this anywhere text needs to be readable against varied backgrounds
    public static Text MakeBoldLabel(Transform parent, string name, string content,
        int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter,
        FontStyle style = FontStyle.Bold,
        Vector2? anchoredPos = null, Vector2? size = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = anchoredPos ?? Vector2.zero;
        rt.sizeDelta = size ?? new Vector2(400, fontSize + 12);

        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.text = content;
        text.raycastTarget = false;

        // Strong black outline (8-direction) for readability on any background
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 1f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Soft shadow for depth
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }
}
