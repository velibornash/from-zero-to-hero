using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ObjectClickInfo : MonoBehaviour
{
    Canvas canvas;
    GameObject panel;
    Text titleText;
    Text bodyText;
    Button closeButton;
    Text headerHint;
    bool visible;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        if (canvas.renderMode == RenderMode.WorldSpace)
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        BuildPanel();
        panel.SetActive(false);
        visible = false;
    }

    void BuildPanel()
    {
        Color woodDark = new Color(0.20f, 0.12f, 0.05f);
        Color woodMid = new Color(0.32f, 0.20f, 0.09f);
        Color parchmentBg = new Color(0.95f, 0.88f, 0.74f);
        Color parchmentDark = new Color(0.78f, 0.62f, 0.40f);
        Color inkDark = new Color(0.18f, 0.10f, 0.03f);
        Color inkBrown = new Color(0.40f, 0.25f, 0.12f);
        Color goldAccent = new Color(0.75f, 0.55f, 0.12f);
        Color bannerRed = new Color(0.55f, 0.12f, 0.08f);

        var shadow = new GameObject("Shadow");
        shadow.transform.SetParent(transform, false);
        var shRt = shadow.AddComponent<RectTransform>();
        shRt.anchorMin = new Vector2(0.5f, 0.5f);
        shRt.anchorMax = new Vector2(0.5f, 0.5f);
        shRt.pivot = new Vector2(0.5f, 0.5f);
        shRt.anchoredPosition = new Vector2(4, -4);
        shRt.sizeDelta = new Vector2(460, 320);
        var shImg = shadow.AddComponent<Image>();
        shImg.color = new Color(0, 0, 0, 0.45f);

        panel = new GameObject("ObjectInfoPanel");
        panel.transform.SetParent(transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = new Vector2(-340, 0);
        pRt.sizeDelta = new Vector2(460, 320);
        var pImg = panel.AddComponent<Image>();
        pImg.color = parchmentBg;
        pImg.raycastTarget = false;

        var frame = new GameObject("Frame");
        frame.transform.SetParent(panel.transform, false);
        var fRt = frame.AddComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.sizeDelta = new Vector2(-8, -8);
        var fImg = frame.AddComponent<Image>();
        fImg.color = woodMid;
        fImg.raycastTarget = false;

        var inner = new GameObject("Inner");
        inner.transform.SetParent(frame.transform, false);
        var iRt = inner.AddComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero;
        iRt.anchorMax = Vector2.one;
        iRt.sizeDelta = new Vector2(-5, -5);
        var iImg = inner.AddComponent<Image>();
        iImg.color = woodDark;
        iImg.raycastTarget = false;

        var innerBorder = new GameObject("InnerBorder");
        innerBorder.transform.SetParent(inner.transform, false);
        var ibRt = innerBorder.AddComponent<RectTransform>();
        ibRt.anchorMin = Vector2.zero;
        ibRt.anchorMax = Vector2.one;
        ibRt.sizeDelta = new Vector2(-6, -6);
        var ibImg = innerBorder.AddComponent<Image>();
        ibImg.color = new Color(0.92f, 0.84f, 0.68f);
        ibImg.raycastTarget = false;

        var banner = new GameObject("Banner");
        banner.transform.SetParent(panel.transform, false);
        var bRt = banner.AddComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 1);
        bRt.anchorMax = new Vector2(1, 1);
        bRt.pivot = new Vector2(0.5f, 1);
        bRt.anchoredPosition = Vector2.zero;
        bRt.sizeDelta = new Vector2(-20, -46);
        var bImg = banner.AddComponent<Image>();
        bImg.color = bannerRed;
        bImg.raycastTarget = false;

        var trimTop = new GameObject("TrimTop");
        trimTop.transform.SetParent(banner.transform, false);
        var ttRt = trimTop.AddComponent<RectTransform>();
        ttRt.anchorMin = new Vector2(0, 1);
        ttRt.anchorMax = new Vector2(1, 1);
        ttRt.pivot = new Vector2(0.5f, 1);
        ttRt.anchoredPosition = Vector2.zero;
        ttRt.sizeDelta = new Vector2(0, -2);
        var ttImg = trimTop.AddComponent<Image>();
        ttImg.color = goldAccent;
        ttImg.raycastTarget = false;

        var trimBot = new GameObject("TrimBot");
        trimBot.transform.SetParent(banner.transform, false);
        var tbRt = trimBot.AddComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 1);
        tbRt.anchorMax = new Vector2(1, 1);
        tbRt.pivot = new Vector2(0.5f, 1);
        tbRt.anchoredPosition = new Vector2(0, -46);
        tbRt.sizeDelta = new Vector2(0, -2);
        var tbImg = trimBot.AddComponent<Image>();
        tbImg.color = goldAccent;
        tbImg.raycastTarget = false;

        titleText = MakeLabel(panel.transform, "Object",
            new Vector2(0, -64), new Vector2(420, 36),
            new Color(1, 1, 0.85f), 26, FontStyle.Bold, TextAnchor.MiddleCenter);

        headerHint = MakeLabel(panel.transform, "[click to inspect]  [ESC / click empty to close]",
            new Vector2(0, -94), new Vector2(420, 18),
            inkBrown, 11, FontStyle.Italic, TextAnchor.MiddleCenter);

        var sep = new GameObject("Separator");
        sep.transform.SetParent(panel.transform, false);
        var sepRt = sep.AddComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(0.5f, 1);
        sepRt.anchorMax = new Vector2(0.5f, 1);
        sepRt.pivot = new Vector2(0.5f, 1);
        sepRt.anchoredPosition = new Vector2(0, -110);
        sepRt.sizeDelta = new Vector2(360, 2);
        var sepImg = sep.AddComponent<Image>();
        sepImg.color = parchmentDark;
        sepImg.raycastTarget = false;

        bodyText = MakeLabel(panel.transform, "",
            new Vector2(0, -123), new Vector2(420, 170),
            inkDark, 14, FontStyle.Normal, TextAnchor.UpperLeft);

        var closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panel.transform, false);
        var cbRt = closeBtnObj.AddComponent<RectTransform>();
        cbRt.anchorMin = new Vector2(0.5f, 0);
        cbRt.anchorMax = new Vector2(0.5f, 0);
        cbRt.pivot = new Vector2(0.5f, 0);
        cbRt.anchoredPosition = new Vector2(0, 8);
        cbRt.sizeDelta = new Vector2(120, 30);
        closeButton = closeBtnObj.AddComponent<Button>();
        var cbImg = closeBtnObj.AddComponent<Image>();
        cbImg.color = woodDark;
        var cbLabel = MakeLabel(closeBtnObj.transform, "Close",
            Vector2.zero, new Vector2(120, 30),
            new Color(1, 0.95f, 0.8f), 14, FontStyle.Bold, TextAnchor.MiddleCenter);
        cbLabel.rectTransform.anchorMin = Vector2.zero;
        cbLabel.rectTransform.anchorMax = Vector2.one;
        cbLabel.rectTransform.sizeDelta = Vector2.zero;
        closeButton.onClick.AddListener(Hide);
    }

    Text MakeLabel(Transform parent, string content, Vector2 pos, Vector2 size,
        Color color, int fontSize, FontStyle fontStyle, TextAnchor anchor)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = anchor;
        text.text = content;
        return text;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var cam = Camera.main;
        if (cam == null) return;

        var mouse = Input.mousePosition;
        var ray = cam.ScreenPointToRay(mouse);
        if (Physics.Raycast(ray, out var hit, 500f))
        {
            Show(hit.collider.gameObject, hit.point);
        }
        else if (visible)
        {
            Hide();
        }
    }

    public void Show(GameObject go, Vector3 worldHit)
    {
        if (panel == null) BuildPanel();

        var name = ResolveName(go);
        titleText.text = name;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>Type</b>  {Describe(go)}");
        sb.AppendLine($"<b>Position</b>  ({worldHit.x:0.0}, {worldHit.y:0.0}, {worldHit.z:0.0})");
        sb.AppendLine();

        if (go.transform.parent != null)
            sb.AppendLine($"<b>Parent</b>  {go.transform.parent.name}");

        float dist = Vector3.Distance(Camera.main.transform.position, worldHit);
        sb.AppendLine($"<b>Distance</b>  {dist:0.0} m");

        var rend = go.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            var b = rend.bounds;
            sb.AppendLine($"<b>Footprint</b>  {b.size.x:0.0} x {b.size.z:0.0} m  (h {b.size.y:0.0})");
        }

        var bz = go.GetComponentInParent<BuildZone>();
        if (bz != null) sb.AppendLine($"<b>Build zone cost</b>  {bz.cost} gold  (built: {bz.isBuilt})");

        var labelText = FindBuildingLabel(go);
        if (!string.IsNullOrEmpty(labelText)) sb.AppendLine($"<b>Label</b>  {labelText}");

        var pctrl = go.GetComponentInParent<PlayerController3D>();
        if (pctrl != null) sb.AppendLine($"<b>Hero controller</b>  speed {pctrl.speed:0.0} m/s");

        var npc = go.GetComponentInParent<NPCPatrol>();
        if (npc != null) sb.AppendLine($"<b>NPC patrol</b>  radius {npc.patrolRadius:0.0}, speed {npc.moveSpeed:0.0}{(npc.idleOnly ? " (idle only)" : "")}");

        var cmbt = go.GetComponentInParent<Combat>();
        if (cmbt != null) sb.AppendLine($"<b>Combat</b>  range {cmbt.range:0.0} m, damage {cmbt.damagePerSec:0.0}/s, max HP {cmbt.maxHp:0}");

        sb.AppendLine();
        sb.AppendLine("<i>Click another object to inspect it, or ESC to close.</i>");

        bodyText.text = sb.ToString();
        bodyText.supportRichText = true;

        panel.SetActive(true);
        visible = true;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        visible = false;
    }

    static string ResolveName(GameObject go)
    {
        if (go == null) return "Unknown";
        var label = FindBuildingLabel(go);
        if (!string.IsNullOrEmpty(label)) return label;
        if (go.transform.parent != null)
        {
            var pname = go.transform.parent.name;
            if (pname.EndsWith("(Clone)")) pname = pname.Substring(0, pname.Length - "(Clone)".Length);
            if (!string.IsNullOrEmpty(pname) && pname != go.name) return $"{pname} \u00b7 {go.name}";
        }
        return go.name;
    }

    static string FindBuildingLabel(GameObject go)
    {
        if (go == null) return null;
        var tm = go.GetComponentInChildren<TextMesh>();
        if (tm != null && !string.IsNullOrEmpty(tm.text)) return tm.text;
        if (go.transform.parent != null)
        {
            tm = go.transform.parent.GetComponentInChildren<TextMesh>();
            if (tm != null && !string.IsNullOrEmpty(tm.text)) return tm.text;
        }
        return null;
    }

    static string Describe(GameObject go)
    {
        if (go == null) return "—";
        if (go.GetComponent<BuildZone>() != null) return "Build zone";
        if (go.GetComponent<PlayerController3D>() != null) return "Hero";
        if (go.GetComponent<NPCPatrol>() != null) return "Villager (NPC)";
        if (go.GetComponent<Combat>() != null) return "Combat unit";
        if (go.GetComponent<CameraFollow3D>() != null) return "Camera";
        if (go.GetComponentInChildren<Light>() != null) return "Light source";
        if (go.GetComponentInChildren<MeshFilter>() != null) return "3D object";
        if (go.name.Contains("River")) return "River / water";
        if (go.name.Contains("Fence") || go.name.Contains("GatePost")) return "Fence segment";
        if (go.name.Contains("Tree") || go.name.Contains("Pine")) return "Tree";
        if (go.name.Contains("Rock")) return "Rock";
        if (go.name.Contains("Wheat")) return "Wheat";
        if (go.name.Contains("Flag")) return "Flag";
        return go.tag ?? "GameObject";
    }
}
