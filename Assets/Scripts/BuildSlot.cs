using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour
{
    public BuildSlotData data;
    public int slotIndex;

    public enum State { Locked, Available, Building, Ready, Built }
    public State state = State.Locked;
    public bool IsBuilt => state == State.Built;

    int spentGold;
    float tickTimer;
    const float TICK_INTERVAL = 0.12f;

    [SerializeField] GameObject tileQuad;
    [SerializeField] Canvas slotCanvas;
    [SerializeField] Image iconImage;
    [SerializeField] Text nameLabel;
    [SerializeField] Text arrowLabel;
    [SerializeField] Image coinImage;
    [SerializeField] Text costLabel;
    [SerializeField] Image progressFill;

    static readonly Color Brown = new Color(0.48f, 0.30f, 0.16f, 0.95f);
    static readonly Color White = new Color(1f, 1f, 1f, 0.95f);

    public void Init(BuildSlotData d, int index, bool unlocked)
    {
        data = d;
        slotIndex = index;
        state = unlocked ? State.Available : State.Locked;
        CleanOldChildren();
        CreateVisuals();
        UpdateVisuals();
    }

    void Start()
    {
        // Runtime failsafe: re-create dynamic sprites if they were lost during scene save.
        if (iconImage != null && iconImage.sprite == null)
            iconImage.sprite = GetBuildingIconSprite();
        if (coinImage != null && coinImage.sprite == null)
            coinImage.sprite = LoadGoldSprite();
        UpdateVisuals();
    }

    void CleanOldChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "TileBase" || child.name == "SlotCanvas")
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
    }

    public void Unlock()
    {
        Debug.Log($"BuildSlot '{name}' (idx={slotIndex}) Unlock called, was state={state}");
        if (state == State.Locked)
        {
            state = State.Available;
            UpdateVisuals();
        }
    }

    void CreateVisuals()
    {
        // ---- Brown rounded square tile on the ground ----
        tileQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tileQuad.name = "TileBase";
        tileQuad.transform.SetParent(transform);
        tileQuad.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        tileQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        tileQuad.transform.localScale = new Vector3(10f, 10f, 1f);
        DestroyImmediate(tileQuad.GetComponent<MeshCollider>());

        var tileMat = new Material(Shader.Find("Unlit/Transparent"));
        tileMat.mainTexture = CreateTileTexture();
        tileMat.color = Color.white;
        tileQuad.GetComponent<Renderer>().sharedMaterial = tileMat;

        // ---- World-space UI canvas (billboarded) ----
        var canvasObj = new GameObject("SlotCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0f, 4.5f, 0f);
        canvasObj.transform.localRotation = Quaternion.identity;
        slotCanvas = canvasObj.AddComponent<Canvas>();
        slotCanvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.AddComponent<Billboard>();

        var canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(320f, 320f);
        canvasObj.transform.localScale = Vector3.one * 0.04f;

        // Progress bar (top)
        var progressBg = MakeUiImage(canvasObj.transform, "ProgressBg",
            new Vector2(0f, 150f), new Vector2(260f, 22f), new Color(0.1f, 0.08f, 0.06f, 0.85f));
        progressBg.raycastTarget = false;
        progressFill = MakeUiImage(canvasObj.transform, "ProgressFill",
            new Vector2(-130f, 150f), new Vector2(260f, 22f), new Color(0.25f, 0.9f, 0.3f, 0.95f));
        progressFill.raycastTarget = false;
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = 0;
        progressFill.fillAmount = 0f;
        progressFill.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

        // Building name + down arrow (above tile)
        nameLabel = MakeUiText(canvasObj.transform, "NameLabel",
            new Vector2(0f, 130f), new Vector2(300f, 80f), 52, new Color(1f, 0.97f, 0.85f), "CHURCH");
        AddOutline(nameLabel, new Color(0.2f, 0.1f, 0.03f), new Vector2(4f, 4f));

        arrowLabel = MakeUiText(canvasObj.transform, "ArrowLabel",
            new Vector2(0f, 70f), new Vector2(90f, 60f), 56, new Color(1f, 0.95f, 0.4f), "\u25BC");
        AddOutline(arrowLabel, new Color(0.2f, 0.1f, 0.03f), new Vector2(4f, 4f));

        // Gold coin + cost (bottom of canvas = near the tile)
        coinImage = MakeUiImage(canvasObj.transform, "CoinIcon",
            new Vector2(-70f, -60f), new Vector2(60f, 60f), Color.white);
        coinImage.sprite = LoadGoldSprite();
        coinImage.raycastTarget = false;

        costLabel = MakeUiText(canvasObj.transform, "CostLabel",
            new Vector2(30f, -60f), new Vector2(140f, 70f), 54, new Color(1f, 0.95f, 0.35f), "0");
        AddOutline(costLabel, new Color(0.2f, 0.1f, 0.03f), new Vector2(4f, 4f));
    }

    Image MakeUiImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    Text MakeUiText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, Color color, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = FontStyle.Bold;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = text;
        txt.raycastTarget = false;
        return txt;
    }

    void AddOutline(Text txt, Color color, Vector2 distance)
    {
        var outline = txt.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    Sprite LoadGoldSprite()
    {
        var sprite = Resources.Load<Sprite>("HUDIcons/gold_icon");
        if (sprite != null) return sprite;

        var tex = CreateCoinTexture(64);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    Texture2D CreateCoinTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color gold = new Color(1f, 0.85f, 0.15f);
        Color goldDark = new Color(0.75f, 0.55f, 0.05f);
        Color shine = new Color(1f, 0.95f, 0.55f);

        int r = size / 2;
        int cx = r, cy = r;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int dx = x - cx, dy = y - cy;
                int d2 = dx * dx + dy * dy;
                if (d2 > r * r) { tex.SetPixel(x, y, clear); continue; }

                if (d2 > (r - 3) * (r - 3)) tex.SetPixel(x, y, goldDark);
                else if (dy > size / 5 && dx < 0) tex.SetPixel(x, y, shine);
                else tex.SetPixel(x, y, gold);
            }
        }
        tex.Apply();
        return tex;
    }

    Sprite GetBuildingIconSprite()
    {
        if (data == null) return null;
        string key = data.slotName.ToLowerInvariant();
        Texture2D tex;
        if (key.Contains("church")) tex = CreateChurchIcon();
        else if (key.Contains("flag")) tex = CreateFlagIcon();
        else if (key.Contains("tower")) tex = CreateTowerIcon();
        else tex = CreateGenericIcon();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    Texture2D CreateGenericIcon()
    {
        int s = 64;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < s; x++)
            for (int y = 0; y < s; y++)
                tex.SetPixel(x, y, clear);
        return tex;
    }

    Texture2D CreateChurchIcon()
    {
        int s = 64;
        var tex = CreateGenericIcon();
        Color roof = new Color(0.75f, 0.2f, 0.12f);
        Color wall = new Color(0.55f, 0.35f, 0.18f);
        Color door = new Color(0.2f, 0.1f, 0.04f);
        Color cross = new Color(1f, 0.9f, 0.35f);

        int baseY = 10, height = 34, width = 30, roofH = 16;
        int cx = s / 2, left = cx - width / 2, right = cx + width / 2;

        // Walls
        for (int x = left; x <= right; x++)
            for (int y = baseY; y < baseY + height; y++)
                tex.SetPixel(x, y, wall);

        // Roof triangle
        for (int y = 0; y < roofH; y++)
        {
            int w = (int)((width / 2f + 2) * (1f - y / (float)roofH));
            for (int x = cx - w; x <= cx + w; x++)
                tex.SetPixel(x, baseY + height + y, roof);
        }

        // Door
        for (int x = cx - 5; x <= cx + 5; x++)
            for (int y = baseY; y < baseY + 14; y++)
                tex.SetPixel(x, y, door);

        // Cross
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = baseY + height + roofH + 2; y < baseY + height + roofH + 12; y++)
                tex.SetPixel(x, y, cross);
        for (int x = cx - 5; x <= cx + 5; x++)
            tex.SetPixel(x, baseY + height + roofH + 7, cross);

        tex.Apply();
        return tex;
    }

    Texture2D CreateFlagIcon()
    {
        var tex = CreateGenericIcon();
        Color pole = new Color(0.55f, 0.55f, 0.55f);
        Color red = new Color(0.9f, 0.08f, 0.08f);
        Color blue = new Color(0.05f, 0.18f, 0.55f);
        Color white = Color.white;

        int poleX = 18, baseY = 8, poleH = 50;
        for (int y = baseY; y < baseY + poleH; y++)
            for (int x = poleX - 2; x <= poleX + 2; x++)
                tex.SetPixel(x, y, pole);

        // Flag waving
        for (int y = baseY + poleH - 8; y < baseY + poleH - 2; y++)
        {
            int wave = (y % 4 < 2) ? 2 : 0;
            for (int x = poleX + 3; x <= poleX + 36 + wave; x++)
                tex.SetPixel(x, y, red);
        }
        for (int y = baseY + poleH - 16; y < baseY + poleH - 8; y++)
        {
            int wave = (y % 4 < 2) ? 2 : 0;
            for (int x = poleX + 3; x <= poleX + 34 + wave; x++)
                tex.SetPixel(x, y, blue);
        }
        for (int y = baseY + poleH - 24; y < baseY + poleH - 16; y++)
        {
            int wave = (y % 4 < 2) ? 2 : 0;
            for (int x = poleX + 3; x <= poleX + 30 + wave; x++)
                tex.SetPixel(x, y, white);
        }

        tex.Apply();
        return tex;
    }

    Texture2D CreateTowerIcon()
    {
        int s = 64;
        var tex = CreateGenericIcon();
        Color stone = new Color(0.55f, 0.55f, 0.52f);
        Color dark = new Color(0.35f, 0.35f, 0.33f);

        int baseY = 8, height = 42, width = 26, cx = s / 2;
        int left = cx - width / 2, right = cx + width / 2;

        // Tower body
        for (int x = left; x <= right; x++)
            for (int y = baseY; y < baseY + height; y++)
                tex.SetPixel(x, y, stone);

        // Crenellations
        int crenW = 5, crenH = 5;
        for (int i = 0; i < 4; i++)
        {
            int cl = left + i * (width / 3) - 1;
            for (int x = cl; x < cl + crenW; x++)
                for (int y = baseY + height; y < baseY + height + crenH; y++)
                    tex.SetPixel(x, y, stone);
        }

        // Door
        for (int x = cx - 5; x <= cx + 5; x++)
            for (int y = baseY; y < baseY + 12; y++)
                tex.SetPixel(x, y, dark);

        tex.Apply();
        return tex;
    }

    Texture2D CreateTileTexture()
    {
        int w = 256, h = 256;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, clear);

        int pad = 10;
        int radius = 36;

        // Rounded brown fill
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < pad || x >= w - pad || y < pad || y >= h - pad) continue;

                float cx = 0f, cy = 0f;
                bool corner = false;
                if (x < pad + radius && y < pad + radius)
                { cx = pad + radius; cy = pad + radius; corner = true; }
                else if (x >= w - pad - radius && y < pad + radius)
                { cx = w - pad - radius - 1; cy = pad + radius; corner = true; }
                else if (x < pad + radius && y >= h - pad - radius)
                { cx = pad + radius; cy = h - pad - radius - 1; corner = true; }
                else if (x >= w - pad - radius && y >= h - pad - radius)
                { cx = w - pad - radius - 1; cy = h - pad - radius - 1; corner = true; }

                if (corner)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy > radius * radius) continue;
                }
                tex.SetPixel(x, y, Brown);
            }
        }

        // White rounded corner brackets
        int bracketLen = 58;
        int bracketThick = 10;
        int inset = pad + radius / 2;
        DrawCornerBracket(tex, inset, inset, 1, 1, bracketLen, bracketThick, White);
        DrawCornerBracket(tex, w - inset - 1, inset, -1, 1, bracketLen, bracketThick, White);
        DrawCornerBracket(tex, inset, h - inset - 1, 1, -1, bracketLen, bracketThick, White);
        DrawCornerBracket(tex, w - inset - 1, h - inset - 1, -1, -1, bracketLen, bracketThick, White);

        tex.Apply();
        return tex;
    }

    void DrawCornerBracket(Texture2D tex, int cx, int cy, int sx, int sy, int len, int thick, Color color)
    {
        // Horizontal arm
        for (int x = cx; x != cx + sx * len; x += sx)
            for (int y = cy; y != cy + sy * thick; y += sy)
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, color);

        // Vertical arm
        for (int y = cy; y != cy + sy * len; y += sy)
            for (int x = cx; x != cx + sx * thick; x += sx)
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, color);

        // Rounded end cap on horizontal arm
        int hx = cx + sx * (len - thick / 2);
        int hy = cy + sy * (thick / 2);
        for (int x = hx - thick; x <= hx + thick; x++)
            for (int y = hy - thick; y <= hy + thick; y++)
                if ((x - hx) * (x - hx) + (y - hy) * (y - hy) <= thick * thick / 2)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                        tex.SetPixel(x, y, color);

        // Rounded end cap on vertical arm
        int vx = cx + sx * (thick / 2);
        int vy = cy + sy * (len - thick / 2);
        for (int x = vx - thick; x <= vx + thick; x++)
            for (int y = vy - thick; y <= vy + thick; y++)
                if ((x - vx) * (x - vx) + (y - vy) * (y - vy) <= thick * thick / 2)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                        tex.SetPixel(x, y, color);
    }

    void Update()
    {
        HandleClickInput();

        if (state == State.Building)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= TICK_INTERVAL)
            {
                tickTimer = 0f;
                if (HUDController.Gold > 0 && spentGold < data.cost)
                {
                    HUDController.Gold--;
                    spentGold++;
                    UpdateVisuals();
                    if (spentGold >= data.cost)
                    {
                        state = State.Ready;
                        HUDController.PushEvent($"{data.slotName} ready! Step off the tile to finish building.");
                        UpdateVisuals();
                    }
                }
            }
        }
    }

    void HandleClickInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, Physics.AllLayers, QueryTriggerInteraction.Collide))
            return;

        if (hit.collider.gameObject != gameObject && hit.collider.GetComponentInParent<BuildSlot>() != this)
            return;

        HandleClick();
    }

    public void HandleClick()
    {
        if (state == State.Available)
        {
            state = State.Building;
            HUDController.PushEvent($"Building {data.slotName}... ({data.cost} gold)");
        }
        else if (state == State.Building)
        {
            int remaining = data.cost - spentGold;
            if (HUDController.Gold >= remaining)
            {
                HUDController.Gold -= remaining;
                spentGold = data.cost;
                state = State.Ready;
                HUDController.PushEvent($"{data.slotName} ready! Step off the tile to finish building.");
            }
        }
        // Ready state intentionally waits for the hero to step off the tile.
        UpdateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (state != State.Available) return;
        if (!other.CompareTag("Player")) return;
        state = State.Building;
        HUDController.PushEvent($"Building {data.slotName}... ({data.cost} gold)");
        UpdateVisuals();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (state == State.Ready)
        {
            FinishBuild();
            return;
        }

        if (state == State.Building)
        {
            state = State.Available;
            HUDController.PushEvent($"Paused {data.slotName}. Spent {spentGold}/{data.cost} gold.");
            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        if (slotCanvas == null) return;

        bool locked = state == State.Locked;
        bool built = state == State.Built;
        bool available = state == State.Available;
        bool building = state == State.Building;
        bool ready = state == State.Ready;

        tileQuad.SetActive(!locked && !built);
        slotCanvas.gameObject.SetActive(!locked && !built);

        if (locked || built) return;

        nameLabel.text = data.slotName.ToUpperInvariant();

        // When paused (Available but partial progress was made), keep showing the remaining cost.
        bool hasProgress = spentGold > 0;
        costLabel.text = (building || ready || hasProgress ? Mathf.Max(0, data.cost - spentGold) : data.cost).ToString();

        if (building)
        {
            progressFill.fillAmount = spentGold / (float)data.cost;
            nameLabel.color = new Color(0.8f, 1f, 0.8f);
        }
        else if (ready)
        {
            progressFill.fillAmount = 1f;
            nameLabel.color = new Color(0.4f, 1f, 0.4f);
            costLabel.text = "0";
        }
        else if (hasProgress)
        {
            progressFill.fillAmount = spentGold / (float)data.cost;
            nameLabel.color = new Color(1f, 0.92f, 0.6f);
        }
        else
        {
            progressFill.fillAmount = 0f;
            nameLabel.color = new Color(1f, 0.97f, 0.85f);
        }
    }

    void FinishBuild()
    {
        try
        {
            state = State.Built;
            UpdateVisuals();
            SpawnBuilding();
            SpawnWorker();
            SmokePuff(transform.position);
            if (data != null)
            {
                HUDController.PushEvent(data.completedMessage);
            }
            // Notify SlotManager FIRST (so fences/etc. build even if popup throws)
            SlotManager.Instance?.OnSlotBuilt(slotIndex);
            // Spawn Mage on mage tile slots (6-9)
            if (slotIndex >= 6 && slotIndex <= 9)
                SpawnMageOnTile();
            // Show popup LAST (won't break the rest if it throws)
            if (data != null)
            {
                try { ShowCompletionPopup(); }
                catch (System.Exception ex) { Debug.LogWarning($"ShowCompletionPopup failed: {ex.Message}"); }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BuildSlot '{name}' FinishBuild failed: {e}");
        }
    }

    void SmokePuff(Vector3 pos)
    {
        var smokeMat = new Material(Shader.Find("Standard"));
        smokeMat.color = new Color(0.95f, 0.95f, 0.9f, 0.8f);
        // Standard transparent setup
        smokeMat.SetFloat("_Mode", 3); // Transparent
        smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        smokeMat.SetInt("_ZWrite", 0);
        smokeMat.DisableKeyword("_ALPHABLEND_ON");
        smokeMat.DisableKeyword("_ALPHATEST_ON");
        smokeMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        smokeMat.EnableKeyword("_ALPHABLEND_ON");
        smokeMat.renderQueue = 3000;

        for (int i = 0; i < 30; i++)
        {
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = "SmokePuff";
            Destroy(puff.GetComponent<SphereCollider>());
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float rad = Random.Range(0.3f, 3f);
            puff.transform.position = pos + new Vector3(
                Mathf.Cos(angle) * rad,
                Random.Range(0.2f, 2f),
                Mathf.Sin(angle) * rad);
            float s = Random.Range(0.8f, 2.5f);
            puff.transform.localScale = Vector3.one * s;
            puff.GetComponent<Renderer>().sharedMaterial = smokeMat;
            StartCoroutine(AnimateSmoke(puff.transform));
        }
    }

    IEnumerator AnimateSmoke(Transform t)
    {
        Vector3 startScale = t.localScale;
        Vector3 drift = new Vector3(
            Random.Range(-0.8f, 0.8f),
            Random.Range(1f, 2.5f),
            Random.Range(-0.8f, 0.8f));
        float duration = 2.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            t.position += drift * Time.deltaTime;
            t.localScale = startScale * Mathf.Lerp(1f, 4f, p) * (1f - p * 0.5f);
            yield return null;
        }
        Destroy(t.gameObject);
    }

    public void PulseUnlock()
    {
        if (slotCanvas == null) return;
        StartCoroutine(PulseUnlockRoutine());
    }

    IEnumerator PulseUnlockRoutine()
    {
        var canvasRt = slotCanvas.GetComponent<RectTransform>();
        Vector3 baseScale = canvasRt.localScale;
        Vector3 bigScale = baseScale * 1.35f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3.5f;
            canvasRt.localScale = Vector3.Lerp(baseScale, bigScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        canvasRt.localScale = baseScale;
    }

    void ShowCompletionPopup()
    {
        Debug.Log($"BuildSlot '{name}': ShowCompletionPopup for slot {slotIndex}");
        string title, body;
        switch (slotIndex)
        {
            case 0: // Church
                title = "Church Raised";
                body = "The heart of the village beats strong.\n\n" +
                       "The church brings faith and community. Villagers gather here for worship and counsel.\n\n" +
                       "Unlocks:\n" +
                       "  • Enemy waves begin\n" +
                       "  • Foundation of the village";
                break;
            case 1: // Flag
                title = "Serbian Banner Flies";
                body = "The tricolor waves above the village.\n\n" +
                       "The Serbian flag — red, blue, white with the coat of arms — unites all villagers under one banner.\n\n" +
                       "Unlocks:\n" +
                       "  • Four corner tower positions";
                break;
            case 2: // Tower SW
            case 3: // Tower SE
            case 4: // Tower NE
            case 5: // Tower NW
                // Map slotIndex 2-5 to corner index 0-3
                int n = slotIndex - 2;
                string[] cornerNames = { "Southwest", "Southeast", "Northeast", "Northwest" };
                if (n >= 0 && n < cornerNames.Length)
                {
                    title = cornerNames[n] + " Tower Built";
                    body = "A new watchtower rises at the " + cornerNames[n].ToLower() + " corner.\n\n" +
                           "The tower automatically fires upon any enemy that comes within range.\n\n" +
                           "Unlocks:\n" +
                           "  • Auto-defense vs wolves and barbarians\n" +
                           "  • After all 4 towers: Mage tiles";
                }
                else
                {
                    title = "Tower Built";
                    body = "A new watchtower rises.";
                }
                break;
            case 6: // Mage tile 1
            case 7: // Mage tile 2
            case 8: // Mage tile 3
            case 9: // Mage tile 4
                int m = slotIndex - 5;
                title = "Mage Joins the Defense";
                body = "A powerful mage takes position at the tile.\n\n" +
                       "Mages cast magic projectiles that deal double damage and fire faster than the towers.\n\n" +
                       "Unlocks:\n" +
                       "  • Enhanced magical defense";
                break;
            default:
                title = data.slotName + " Built";
                body = data.completedMessage;
                break;
        }
        Debug.Log($"BuildSlot '{name}': Calling BuildingPopup.Show with title='{title}'");
        BuildingPopup.Show(title, body, "default", transform.position);
    }

    void SpawnBuilding()
    {
        if (data == null)
        {
            Debug.LogError($"BuildSlot '{name}': data is null!");
            return;
        }
        if (data.buildingPrefab == null)
        {
            Debug.LogError($"BuildSlot '{name}': buildingPrefab is null!");
            return;
        }

        Debug.Log($"BuildSlot '{name}': spawning '{data.buildingPrefab.name}' at {transform.position}");

        var building = (GameObject)Object.Instantiate(
            data.buildingPrefab,
            transform.position,
            Quaternion.Euler(data.rotation));

        if (building == null)
        {
            Debug.LogError($"BuildSlot '{name}': Instantiate returned null!");
            return;
        }

        building.name = data.slotName;
        building.transform.localScale = data.scale;

        var filters = building.GetComponentsInChildren<MeshFilter>();
        foreach (var f in filters)
        {
            if (f.sharedMesh == null) continue;
            if (f.gameObject.GetComponent<MeshCollider>() != null) continue;
            var mc = f.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = f.sharedMesh;
            mc.convex = false;
        }

        // Corner tower slots (index 2-5) get tower shooter
        if (slotIndex >= 2 && slotIndex <= 5)
        {
            var shooter = building.AddComponent<TowerShooter>();
            shooter.range = 30f;
            shooter.fireRate = 0.7f;
            shooter.damage = 1;
            shooter.projectileSpeed = 25f;
            shooter.projectileColor = new Color(0.9f, 0.8f, 0.3f);
        }

        StartCoroutine(ScalePuff(building.transform));
        Debug.Log($"BuildSlot '{name}': '{building.name}' spawned successfully.");
    }

    void SpawnWorker()
    {
        var worker = new GameObject("Worker_" + data.slotName);
        worker.transform.position = transform.position + new Vector3(2.2f, 0f, 2.2f);
        worker.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(worker.transform);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
        body.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.3f, 0.45f, 0.7f));
        Destroy(body.GetComponent<CapsuleCollider>());

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(worker.transform);
        head.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        head.transform.localScale = Vector3.one * 0.4f;
        head.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.8f, 0.65f));
        Destroy(head.GetComponent<SphereCollider>());

        var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hat.name = "Hat";
        hat.transform.SetParent(worker.transform);
        hat.transform.localPosition = new Vector3(0f, 2.05f, 0f);
        hat.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
        hat.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.2f, 0.15f, 0.1f));
        Destroy(hat.GetComponent<CapsuleCollider>());

        var patrol = worker.AddComponent<NPCPatrol>();
        patrol.idleOnly = true;
        patrol.patrolRadius = 2f;
        patrol.moveSpeed = 0f;
    }

    void SpawnMageOnTile()
    {
        // Place a glowing rune on the tile to mark the mage
        var rune = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rune.name = "MageRune";
        rune.transform.position = transform.position + Vector3.up * 0.05f;
        rune.transform.localScale = new Vector3(1.4f, 0.05f, 1.4f);
        var runeMat = new Material(Shader.Find("Standard"));
        runeMat.color = new Color(0.4f, 0.2f, 0.9f, 0.7f);
        runeMat.SetFloat("_Mode", 3);
        runeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        runeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        runeMat.SetInt("_ZWrite", 0);
        runeMat.EnableKeyword("_ALPHABLEND_ON");
        runeMat.renderQueue = 3000;
        rune.GetComponent<Renderer>().sharedMaterial = runeMat;
        Object.DestroyImmediate(rune.GetComponent<CapsuleCollider>());

        // Load the KayKit Mage FBX
        string magePath = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE/Characters/fbx/Mage.fbx";
        GameObject mage = null;
#if UNITY_EDITOR
        var magePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(magePath);
        if (magePrefab != null)
        {
            mage = (GameObject)Object.Instantiate(magePrefab, transform.position, Quaternion.identity);
            mage.name = "TowerMage";
            mage.transform.localScale = Vector3.one * 2.0f;
        }
        else
        {
            Debug.LogWarning("KayKit Mage.fbx not found at " + magePath);
        }
#endif
        if (mage == null) return;

        // Add a shooter component to the mage
        var shooter = mage.AddComponent<TowerShooter>();
        shooter.range = 35f;
        shooter.fireRate = 0.45f;
        shooter.damage = 2;
        shooter.projectileSpeed = 30f;
        shooter.projectileColor = new Color(0.6f, 0.3f, 1f);
        shooter.isUpgraded = false;
        shooter.enabled = true;
    }

    Material MakeMat(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }

    IEnumerator ScalePuff(Transform t)
    {
        Vector3 targetScale = t.localScale;
        t.localScale = Vector3.zero;
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float overshoot = 1f + 0.25f * Mathf.Sin(p * Mathf.PI);
            t.localScale = targetScale * (p * overshoot);
            yield return null;
        }
        t.localScale = targetScale;
    }
}
