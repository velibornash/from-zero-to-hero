using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;
    public static int Day = 1;
    public static bool IsNight = false;

    [Header("Timing")]
    public float dayDurationMinutes = 3f;
    public float nightDurationMinutes = 2f;

    [Header("Sun")]
    public Light directionalLight;
    public Gradient lightColorGradient;
    public AnimationCurve lightIntensityCurve;

    [Header("Ambient")]
    public Gradient ambientColorGradient;

    [Header("Fog")]
    public Gradient fogColorGradient;

    float timeOfDay;
    float fullDayLength;
    bool nightPhase;

    Text dayText;
    Image dayIcon;

    void Awake()
    {
        Instance = this;
        fullDayLength = (dayDurationMinutes + nightDurationMinutes) * 60f;
        timeOfDay = dayDurationMinutes * 60f * 0.25f;
        nightPhase = false;
    }

    void Start()
    {
        if (directionalLight == null)
            directionalLight = FindFirstObjectByType<Light>();
        CreateDayUI();
    }

    void Update()
    {
        timeOfDay += Time.deltaTime;
        float dayLen = nightPhase ? nightDurationMinutes * 60f : dayDurationMinutes * 60f;

        if (timeOfDay >= dayLen)
        {
            timeOfDay = 0f;
            nightPhase = !nightPhase;
            if (!nightPhase)
            {
                Day++;
                Debug.Log($"[DayNight] Day {Day} begins");
            }
            IsNight = nightPhase;
        }

        float t = timeOfDay / dayLen;
        UpdateLighting(t);
        UpdateUI();
    }

    void UpdateLighting(float t)
    {
        if (directionalLight != null)
        {
            float sunAngle = Mathf.Lerp(0f, 180f, t);
            if (nightPhase) sunAngle += 180f;
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 170f, 0f);

            if (lightColorGradient != null)
                directionalLight.color = lightColorGradient.Evaluate(t);
            if (lightIntensityCurve != null)
                directionalLight.intensity = lightIntensityCurve.Evaluate(t);
        }

        if (ambientColorGradient != null)
            RenderSettings.ambientLight = ambientColorGradient.Evaluate(t);

        if (fogColorGradient != null)
            RenderSettings.fogColor = fogColorGradient.Evaluate(t);
    }

    void CreateDayUI()
    {
        Canvas canvas = null;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c;
                break;
            }
        }
        if (canvas == null) return;

        var dayGo = new GameObject("DayIndicator");
        dayGo.transform.SetParent(canvas.transform, false);
        var rt = dayGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -8);
        rt.sizeDelta = new Vector2(200, 32);

        dayIcon = dayGo.AddComponent<Image>();
        dayIcon.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        dayIcon.raycastTarget = false;

        var txtGo = new GameObject("DayText");
        txtGo.transform.SetParent(dayGo.transform, false);
        var txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        dayText = txtGo.AddComponent<Text>();
        dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dayText.fontSize = 16;
        dayText.fontStyle = FontStyle.Bold;
        dayText.color = new Color(0.15f, 0.08f, 0.02f);
        dayText.alignment = TextAnchor.MiddleCenter;
        dayText.text = "Day 1";
        dayText.raycastTarget = false;
        var outline = txtGo.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.5f);
        outline.effectDistance = new Vector2(1, 1);
    }

    void UpdateUI()
    {
        if (dayText != null)
        {
            string phase = nightPhase ? "Night" : "Day";
            dayText.text = $"Day {Day} - {phase}";
            if (dayIcon != null)
                dayIcon.color = nightPhase
                    ? new Color(0.2f, 0.2f, 0.4f, 0.8f)
                    : new Color(1f, 0.85f, 0.3f, 0.9f);
        }
    }
}
