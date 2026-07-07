using UnityEngine;

public static class TextureHelper
{
    public static Texture2D ChromaKey(Texture2D source)
    {
        var tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        var pixels = source.GetPixels();
        int w = source.width, h = source.height;

        float sr = 0, sg = 0, sb = 0;
        int samples = 0;
        for (int y = 0; y < h; y += Mathf.Max(1, h / 8))
        {
            for (int x = 0; x < w; x += Mathf.Max(1, w / 8))
            {
                bool corner = (x < w / 6 || x > w * 5 / 6 || y < h / 6 || y > h * 5 / 6);
                if (!corner) continue;
                int idx = y * w + x;
                sr += pixels[idx].r; sg += pixels[idx].g; sb += pixels[idx].b;
                samples++;
            }
        }

        Color bg = samples > 0 ? new Color(sr / samples, sg / samples, sb / samples) : Color.white;

        float threshold = 0.3f;
        for (int i = 0; i < pixels.Length; i++)
        {
            float d = Mathf.Abs(pixels[i].r - bg.r) + Mathf.Abs(pixels[i].g - bg.g) + Mathf.Abs(pixels[i].b - bg.b);
            pixels[i].a = d < threshold ? 0f : 1f;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
