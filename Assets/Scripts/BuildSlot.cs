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
    Transform mageFirePoint;

    [SerializeField] GameObject tileQuad;
    [SerializeField] Canvas slotCanvas;
    [SerializeField] Image iconImage;
    [SerializeField] Text arrowLabel;
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
        if (iconImage != null && iconImage.sprite == null)
            iconImage.sprite = GetBuildingIconSprite();
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

        // Progress bar (top) with gold frame
        var progressFrame = MakeUiImage(canvasObj.transform, "ProgressFrame",
            new Vector2(0f, 150f), new Vector2(268f, 30f), new Color(0, 0, 0, 0));
        progressFrame.raycastTarget = false;
        var pFrameImg = progressFrame.GetComponent<Image>();
        pFrameImg.sprite = UIStyleHelper.Make9SliceBorder(96, 96, 8, 12);
        pFrameImg.type = Image.Type.Sliced;
        pFrameImg.color = Color.white;

        var progressBg = MakeUiImage(progressFrame.transform, "ProgressBg",
            new Vector2(0f, 0f), new Vector2(252f, 14f), new Color(0.08f, 0.06f, 0.04f, 0.9f));
        progressBg.raycastTarget = false;
        progressFill = MakeUiImage(progressFrame.transform, "ProgressFill",
            new Vector2(-126f, 0f), new Vector2(252f, 14f), new Color(0.25f, 0.9f, 0.3f, 0.95f));
        progressFill.raycastTarget = false;
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = 0;
        progressFill.fillAmount = 0f;
        progressFill.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

        // Building icon (replaces name label)
        iconImage = MakeUiImage(canvasObj.transform, "IconImage",
            new Vector2(0f, 130f), new Vector2(104f, 104f), Color.white);
        iconImage.sprite = GetBuildingIconSprite();
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = true;

        arrowLabel = MakeUiText(canvasObj.transform, "ArrowLabel",
            new Vector2(0f, 70f), new Vector2(90f, 60f), 56, new Color(1f, 0.95f, 0.4f), "\u25BC");
        AddOutline(arrowLabel, new Color(0.2f, 0.1f, 0.03f), new Vector2(4f, 4f));

        costLabel = MakeUiText(canvasObj.transform, "CostLabel",
            new Vector2(0f, -40f), new Vector2(120f, 60f), 54, new Color(1f, 0.95f, 0.35f), "0");
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

    Sprite GetBuildingIconSprite()
    {
        if (data == null) return null;
        string key = data.slotName.ToLowerInvariant();
        string iconName = null;
        if (key.Contains("church")) iconName = "church_icon";
        else if (key.Contains("flag")) iconName = "flag_icon";
        else if (key.Contains("tower")) iconName = "tower_icon";
        else if (key.Contains("mage")) iconName = "mage_character_icon";
        else if (key.Contains("ranger") || key.Contains("archer")) iconName = "ranger_archer_icon";
        else if (key.Contains("house")) iconName = "house_icon";
        else if (key.Contains("market")) iconName = "market_icon";
        else if (key.Contains("barracks")) iconName = "barracks_icon";
        else if (key.Contains("sawmill")) iconName = "sawmill_icon";
        else if (key.Contains("windmill")) iconName = "windmill_icon";
        else if (key.Contains("smithy")) iconName = "smithy_icon";
        else if (key.Contains("warehouse")) iconName = "warehouse_icon";
        else if (key.Contains("granary")) iconName = "granary_icon";
        else if (key.Contains("armory")) iconName = "armory_icon";
        else if (key.Contains("stonemason")) iconName = "stonemason_icon";
        else if (key.Contains("townhall")) iconName = "townhall_icon";
        else if (key.Contains("bakery")) iconName = "bakery_icon";

        if (!string.IsNullOrEmpty(iconName))
        {
            var tex = Resources.Load<Texture2D>("BuildingIcons/" + iconName);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        var fallback = Resources.Load<Texture2D>("HUDIcons/stone_icon");
        if (fallback != null)
            return Sprite.Create(fallback, new Rect(0, 0, fallback.width, fallback.height), new Vector2(0.5f, 0.5f));
        return null;
    }

    Texture2D CreateTileTexture()
    {
        var tex = Resources.Load<Texture2D>("Tiles/tile");
        if (tex != null) return tex;

        int w = 256, h = 256;
        tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, clear);

        int pad = 10;
        int radius = 36;

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
        for (int x = cx; x != cx + sx * len; x += sx)
            for (int y = cy; y != cy + sy * thick; y += sy)
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, color);

        for (int y = cy; y != cy + sy * len; y += sy)
            for (int x = cx; x != cx + sx * thick; x += sx)
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, color);

        int hx = cx + sx * (len - thick / 2);
        int hy = cy + sy * (thick / 2);
        for (int x = hx - thick; x <= hx + thick; x++)
            for (int y = hy - thick; y <= hy + thick; y++)
                if ((x - hx) * (x - hx) + (y - hy) * (y - hy) <= thick * thick / 2)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                        tex.SetPixel(x, y, color);

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

        iconImage.sprite = GetBuildingIconSprite();

        bool hasProgress = spentGold > 0;
        costLabel.text = (building || ready || hasProgress ? Mathf.Max(0, data.cost - spentGold) : data.cost).ToString();

        if (building || hasProgress)
        {
            float pct = spentGold / (float)data.cost;
            progressFill.fillAmount = pct;
            progressFill.color = pct >= 1f
                ? new Color(0.25f, 0.9f, 0.3f, 0.95f)
                : new Color(0.9f, 0.85f, 0.15f, 0.95f);
        }
        else if (ready)
        {
            progressFill.fillAmount = 1f;
            progressFill.color = new Color(0.25f, 0.9f, 0.3f, 0.95f);
            costLabel.text = "0";
        }
        else
        {
            progressFill.fillAmount = 0f;
        }
    }

    void FinishBuild()
    {
        try
        {
            state = State.Built;
            UpdateVisuals();
            // Mage tiles (6-9) have no building prefab — skip SpawnBuilding for them
            if (slotIndex < 6 || slotIndex > 9)
                SpawnBuilding();
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
            // Spawn worker for resource buildings (0=church, 1=flag)
            if (slotIndex <= 1)
                SpawnWorker();
            // Show elder popup
            if (data != null)
            {
                try
                {
                    string elderMsg = GetElderMessage();
                    if (elderMsg != null)
                        ElderPopup.Instance?.Show(elderMsg, 5f);
                    ShowCompletionPopup();
                }
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
                title = "The Church Rises";
                body = "The bell tower stands against the sky.\n\n" +
                       "Villagers emerge from the forest, drawn by the sound\n" +
                       "of hammers and hope. The church is more than stone\n" +
                       "and mortar — it is the soul of the village.\n\n" +
                       "But the wild hears it too. Wolves stir in the shadows.\n" +
                       "Barbarian scouts turn their gaze toward the valley.\n\n" +
                       "The fight for this land has begun.";
                break;
            case 1: // Flag
                title = "The Serbian Banner Rises";
                body = "Red, blue, and white — the tricolor snaps in the\n" +
                       "wind above the village. The coat of arms gleams,\n" +
                       "a promise to every Serb who sees it:\n\n" +
                       "\"Here we stand. Here we build.\"\n\n" +
                       "Villagers cheer. Warriors take up arms.\n" +
                       "Four corner foundations are now ready.\n" +
                       "Raise the towers, and let none pass.";
                break;
            case 2: // Tower SW
            case 3: // Tower SE
            case 4: // Tower NE
            case 5: // Tower NW
                int n = slotIndex - 2;
                string[] cornerNames = { "Southwest", "Southeast", "Northeast", "Northwest" };
                if (n >= 0 && n < cornerNames.Length)
                {
                    string corner = cornerNames[n];
                    title = corner + " Watchtower";
                    body = "The " + corner.ToLower() + " watchtower rises high.\n\n" +
                           "Arrows and stones fly at any enemy who dares\n" +
                           "approach this corner of the valley.\n\n" +
                           "One by one, the walls grow teeth.\n" +
                           "When all four towers stand, the village\n" +
                           "will be ready for something greater.";
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
                title = "A Mage Answers the Call";
                body = "A figure in violet robes steps onto the tile.\n\n" +
                       "The air crackles with arcane energy. Purple flames\n" +
                       "dance at their fingertips.\n\n" +
                       "Mages hurl bolts of pure magic that tear through\n" +
                       "enemies faster than any arrow — double the damage\n" +
                       "of a standard tower.\n\n" +
                       "The village grows stronger still.";
                break;
            default:
                title = data.slotName + " Built";
                body = data.completedMessage;
                break;
        }
        Debug.Log($"BuildSlot '{name}': Calling BuildingPopup.Show with title='{title}'");
        string iconKey = data != null ? data.slotName.ToLowerInvariant() : "default";
        BuildingPopup.Show(title, body, iconKey, transform.position);
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

        var ai = worker.AddComponent<WorkerAI>();
        ai.resourceType = "wood";
        ai.gatherInterval = 12f;
        ai.gatherRange = 25f;
        ai.gatherAmount = 2;
    }

    string GetElderMessage()
    {
        if (slotIndex == 0)
            return "The church is raised! Our village has a heart. Now build the Serbian flag to show our pride!";
        if (slotIndex == 1)
            return "The banner flies high! Our watchtowers can now be built to protect the village.";
        if (slotIndex >= 2 && slotIndex <= 5)
            return "A watchtower stands guard! The village is safer. Keep building to unlock the mages.";
        if (slotIndex >= 6 && slotIndex <= 9)
            return "A mage joins our defense! Their magic will smite our enemies.";
        return null;
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
        Object.Destroy(rune.GetComponent<CapsuleCollider>());

        // Create the mage GameObject (try Resources prefab, fallback to stand-in)
        GameObject mage;
        var magePrefab = Resources.Load<GameObject>("Mage");
        if (magePrefab != null)
        {
            mage = (GameObject)Object.Instantiate(magePrefab, transform.position, Quaternion.identity);
            mage.name = "TowerMage";
            mage.transform.localScale = Vector3.one * 2.0f;

            // Attach a magical staff to the right hand
            var staffPrefab = Resources.Load<GameObject>("MageStaff");
            if (staffPrefab != null)
            {
                var staff = (GameObject)Object.Instantiate(staffPrefab);
                staff.name = "MageStaff";
                foreach (var rend in staff.GetComponentsInChildren<Renderer>())
                {
                    var mats = rend.sharedMaterials;
                    for (int mi = 0; mi < mats.Length; mi++)
                    {
                        var newMat = new Material(Shader.Find("Standard"));
                        newMat.name = "StaffMat_" + rend.name + "_" + mi;
                        if (mi == 0)
                            newMat.color = new Color(0.45f, 0.3f, 0.1f);
                        else
                            newMat.color = new Color(0.5f, 0.2f, 0.8f);
                        mats[mi] = newMat;
                    }
                    rend.sharedMaterials = mats;
                }
                var handSlot = FindHandSlot(mage.transform);
                if (handSlot != null)
                {
                    staff.transform.SetParent(handSlot);
                    staff.transform.localPosition = new Vector3(0, 0, 0.5f);
                    staff.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    staff.transform.localScale = Vector3.one * 0.6f;
                }
                else
                {
                    staff.transform.SetParent(mage.transform);
                    staff.transform.localPosition = new Vector3(0.8f, 1.5f, 0.3f);
                    staff.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    staff.transform.localScale = Vector3.one * 0.6f;
                }
                var fp = new GameObject("FirePoint");
                fp.transform.SetParent(staff.transform);
                fp.transform.localPosition = new Vector3(0, 0, 0.7f);
                mageFirePoint = fp.transform;
            }
        }
        else
        {
            Debug.LogWarning("Mage prefab not found in Resources — using fallback.");
            mage = CreateMageFallback();
        }

        // Add a shooter component to the mage — shoots purple fireballs from hand height
        var shooter = mage.AddComponent<TowerShooter>();
        shooter.range = 35f;
        shooter.fireRate = 0.45f;
        shooter.damage = 2;
        shooter.projectileSpeed = 30f;
        shooter.projectileSpawnHeight = 2.5f;
        shooter.firePoint = mageFirePoint;
        shooter.projectileColor = new Color(0.6f, 0.3f, 1f);
        shooter.isUpgraded = false;
        shooter.enabled = true;
    }

    GameObject CreateMageFallback()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "TowerMage";
        go.transform.position = transform.position;
        go.transform.localScale = new Vector3(1.2f, 2.6f, 1.2f);
        var rend = go.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.5f, 0.2f, 0.9f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.5f, 0.2f, 0.9f) * 0.6f);
        rend.sharedMaterial = mat;
        Object.Destroy(go.GetComponent<CapsuleCollider>());
        return go;
    }

    Material MakeMat(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }

    Transform FindHandSlot(Transform parent)
    {
        string name = parent.name.ToLowerInvariant();
        if (name.Contains("handslot") && (name.EndsWith("r") || name.EndsWith("_r")))
            return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var result = FindHandSlot(parent.GetChild(i));
            if (result != null) return result;
        }
        return null;
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
