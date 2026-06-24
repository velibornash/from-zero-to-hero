using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Transform target;
    public float height = 100f;
    public float mapSize = 200f;
    public int texSize = 200;

    Camera minimapCam;
    RenderTexture rt;
    RawImage rawImage;

    void Start()
    {
        if (target == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            if (player != null) target = player.transform;
        }

        // Create render texture
        rt = new RenderTexture(texSize, texSize, 24);
        rt.Create();

        // Create minimap camera
        var go = new GameObject("MinimapCamera");
        minimapCam = go.AddComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = mapSize * 0.5f;
        minimapCam.transform.position = new Vector3(0, height, 0);
        minimapCam.transform.rotation = Quaternion.Euler(90, 0, 0);
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = new Color(0.15f, 0.2f, 0.1f);
        minimapCam.targetTexture = rt;
        minimapCam.depth = -1;

        // Create UI RawImage
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        rawImage = new GameObject("Minimap").AddComponent<RawImage>();
        rawImage.transform.SetParent(canvas.transform, false);
        var rtRt = rawImage.GetComponent<RectTransform>();
        rtRt.anchorMin = new Vector2(1, 0);
        rtRt.anchorMax = new Vector2(1, 0);
        rtRt.pivot = new Vector2(1, 0);
        rtRt.anchoredPosition = new Vector2(-12, 12);
        rtRt.sizeDelta = new Vector2(texSize + 8, texSize + 8);
        rawImage.texture = rt;
        rawImage.color = Color.white;
        rawImage.raycastTarget = false;

        // Border
        var border = new GameObject("MinimapBorder");
        border.transform.SetParent(rawImage.transform, false);
        var bRt = border.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.sizeDelta = Vector2.zero;
        var bImg = border.AddComponent<Image>();
        bImg.color = new Color(0.3f, 0.2f, 0.1f, 0.8f);
        bImg.type = Image.Type.Sliced;
        bImg.raycastTarget = false;
    }

    void LateUpdate()
    {
        if (minimapCam != null && target != null)
        {
            Vector3 pos = target.position;
            minimapCam.transform.position = new Vector3(pos.x, height, pos.z);
        }
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}
