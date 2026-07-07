using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileControls : MonoBehaviour
{
    GameObject runButton;
    GameObject interactButton;
    bool isMobile;

    void Start()
    {
        isMobile = Application.isMobilePlatform || Input.touchSupported;
        if (!isMobile) return;

        var canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        BuildJoystick();
        BuildActionButtons();
    }

    bool IsMobile()
    {
        return isMobile;
    }

    void BuildJoystick()
    {
        var canvas = GetComponent<Canvas>();

        // Joystick base
        var bg = new GameObject("Joystick");
        bg.transform.SetParent(transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0);
        bgRt.anchorMax = new Vector2(0, 0);
        bgRt.pivot = new Vector2(0.5f, 0.5f);
        bgRt.anchoredPosition = new Vector2(160, 160);
        bgRt.sizeDelta = new Vector2(160, 160);
        var bgImg = bg.AddComponent<Image>();
        bgImg.sprite = MakeDiskSprite(160, 0.5f, 0.4f, 0.2f, 0.55f);
        bgImg.color = Color.white;
        bgImg.raycastTarget = true;

        // Gold ring
        var ring = new GameObject("Ring");
        ring.transform.SetParent(bg.transform, false);
        var rRt = ring.AddComponent<RectTransform>();
        rRt.anchorMin = Vector2.zero;
        rRt.anchorMax = Vector2.one;
        rRt.offsetMin = new Vector2(-4, -4);
        rRt.offsetMax = new Vector2(4, 4);
        var rImg = ring.AddComponent<Image>();
        rImg.sprite = MakeRingSprite(160, 0.9f, 0.65f, 0.25f, 0.95f);
        rImg.color = Color.white;
        rImg.raycastTarget = false;

        // Joystick handle
        var handle = new GameObject("Handle");
        handle.transform.SetParent(bg.transform, false);
        var hRt = handle.AddComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0.5f, 0.5f);
        hRt.anchorMax = new Vector2(0.5f, 0.5f);
        hRt.pivot = new Vector2(0.5f, 0.5f);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta = new Vector2(72, 72);
        var hImg = handle.AddComponent<Image>();
        hImg.sprite = MakeDiskSprite(72, 0.92f, 0.72f, 0.28f, 1f);
        hImg.color = Color.white;
        hImg.raycastTarget = false;

        // Handle inner
        var inner = new GameObject("Inner");
        inner.transform.SetParent(handle.transform, false);
        var iRt = inner.AddComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero;
        iRt.anchorMax = Vector2.one;
        iRt.offsetMin = new Vector2(10, 10);
        iRt.offsetMax = new Vector2(-10, -10);
        var iImg = inner.AddComponent<Image>();
        iImg.sprite = MakeDiskSprite(52, 0.55f, 0.35f, 0.10f, 1f);
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

    void BuildActionButtons()
    {
        // Run button (bottom-right)
        runButton = new GameObject("RunButton");
        runButton.transform.SetParent(transform, false);
        var runRt = runButton.AddComponent<RectTransform>();
        runRt.anchorMin = new Vector2(1, 0);
        runRt.anchorMax = new Vector2(1, 0);
        runRt.pivot = new Vector2(1, 0);
        runRt.anchoredPosition = new Vector2(-20, 20);
        runRt.sizeDelta = new Vector2(100, 100);
        var runImg = runButton.AddComponent<Image>();
        runImg.sprite = MakeDiskSprite(100, 0.6f, 0.3f, 0.1f, 0.85f);
        runImg.color = Color.white;
        runImg.raycastTarget = true;
        var runBtn = runButton.AddComponent<Button>();
        runBtn.onClick.AddListener(ToggleRun);

        var runText = new GameObject("RunLabel");
        runText.transform.SetParent(runButton.transform, false);
        var runTextRt = runText.AddComponent<RectTransform>();
        runTextRt.anchorMin = Vector2.zero;
        runTextRt.anchorMax = Vector2.one;
        runTextRt.offsetMin = Vector2.zero;
        runTextRt.offsetMax = Vector2.zero;
        var runLabel = runText.AddComponent<Text>();
        runLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        runLabel.fontSize = 24;
        runLabel.fontStyle = FontStyle.Bold;
        runLabel.color = new Color(1f, 0.95f, 0.55f);
        runLabel.alignment = TextAnchor.MiddleCenter;
        runLabel.text = "RUN";
        runLabel.raycastTarget = false;

        // Interact button (bottom-right, above run button)
        interactButton = new GameObject("InteractButton");
        interactButton.transform.SetParent(transform, false);
        var intRt = interactButton.AddComponent<RectTransform>();
        intRt.anchorMin = new Vector2(1, 0);
        intRt.anchorMax = new Vector2(1, 0);
        intRt.pivot = new Vector2(1, 0);
        intRt.anchoredPosition = new Vector2(-20, 140);
        intRt.sizeDelta = new Vector2(100, 100);
        var intImg = interactButton.AddComponent<Image>();
        intImg.sprite = MakeDiskSprite(100, 0.5f, 0.15f, 0.1f, 0.85f);
        intImg.color = Color.white;
        intImg.raycastTarget = true;
        var intBtn = interactButton.AddComponent<Button>();
        intBtn.onClick.AddListener(Interact);

        var intText = new GameObject("InteractLabel");
        intText.transform.SetParent(interactButton.transform, false);
        var intTextRt = intText.AddComponent<RectTransform>();
        intTextRt.anchorMin = Vector2.zero;
        intTextRt.anchorMax = Vector2.one;
        intTextRt.offsetMin = Vector2.zero;
        intTextRt.offsetMax = Vector2.zero;
        var intLabel = intText.AddComponent<Text>();
        intLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        intLabel.fontSize = 28;
        intLabel.fontStyle = FontStyle.Bold;
        intLabel.color = new Color(1f, 0.9f, 0.4f);
        intLabel.alignment = TextAnchor.MiddleCenter;
        intLabel.text = "E";
        intLabel.raycastTarget = false;
    }

    void ToggleRun()
    {
        PlayerController3D.MobileRun = !PlayerController3D.MobileRun;
        var img = runButton.GetComponent<Image>();
        if (img != null)
            img.color = PlayerController3D.MobileRun
                ? new Color(0.3f, 0.7f, 0.3f, 0.85f)
                : new Color(0.6f, 0.3f, 0.1f, 0.85f);
    }

    void Interact()
    {
        PlayerController3D.MobileInteract = true;
    }

    void Update()
    {
        if (!isMobile) return;

        // Tap-to-move: on touch end, raycast to ground on left half
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended)
            {
                // Ignore right half (camera control area) and UI hits
                if (t.position.x < Screen.width * 0.5f)
                {
                    Ray ray = Camera.main.ScreenPointToRay(t.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 200f))
                    {
                        PlayerController3D.MoveTarget = hit.point;
                    }
                }
            }
        }
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
