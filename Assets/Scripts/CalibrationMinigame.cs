using UnityEngine;
using UnityEngine.UI;

/// <summary>First-session room scan requiring both time and a sufficiently large floor.</summary>
public class CalibrationMinigame : MonoBehaviour
{
    [Header("Requirements")]
    [Min(0f)] public float MinimumScanSeconds = 8f;
    [Min(0.1f)] public float MinimumFloorArea = 2.5f;

    [Header("Placeholder UI")]
    [TextArea] public string ScanningMessage = "Cammina nella stanza per analizzarla...";
    [Range(0f, 1f)] public float MaximumDirtOpacity = 0.72f;

    [Header("Debug")]
    [Tooltip("Mostra le mesh e i bordi dei piani AR durante il Play Mode.")]
    public bool ShowPlaneVisibility;

    public bool IsCalibrated { get; private set; }
    public float Progress { get; private set; }
    public float CurrentFloorArea { get; private set; }

    private float scanTime;
    private CanvasGroup dirtGroup;
    private Text messageText;

    private void Start()
    {
        CreatePlaceholderUI();
        BeginCalibrationIfNeeded();
    }

    private void Update()
    {
        if (ARManager.Singleton == null)
            return;

        ARManager.Singleton.ShowPlanes = ShowPlaneVisibility;

        if (IsCalibrated)
            return;

        scanTime += Time.deltaTime;
        bool hasValidFloor = ARManager.Singleton.TryFindFloor(MinimumFloorArea, out PlaneVisibilityBH floor, out float floorArea);
        CurrentFloorArea = floorArea;
        float timeProgress = MinimumScanSeconds <= 0f ? 1f : Mathf.Clamp01(scanTime / MinimumScanSeconds);
        float areaProgress = Mathf.Clamp01(CurrentFloorArea / MinimumFloorArea);
        Progress = Mathf.Min(timeProgress, areaProgress);

        dirtGroup.alpha = Mathf.Lerp(MaximumDirtOpacity, 0f, Progress);
        messageText.text = $"{ScanningMessage}\n\nTempo: {timeProgress:P0} | Pavimento: {areaProgress:P0}";

        if (hasValidFloor && timeProgress >= 1f)
            CompleteCalibration(floor);
    }

    /// <summary>Call this when returning to calibration. A completed app session is skipped.</summary>
    public void BeginCalibrationIfNeeded()
    {
        if (IsCalibrated)
        {
            dirtGroup.gameObject.SetActive(false);
            return;
        }

        scanTime = 0f;
        dirtGroup.gameObject.SetActive(true);
    }

    private void CompleteCalibration(PlaneVisibilityBH floor)
    {
        IsCalibrated = true;
        Progress = 1f;
        ARManager.Singleton.SetFloorPlane(floor);
        dirtGroup.gameObject.SetActive(false);
        Debug.Log($"Room calibration complete. Floor area: {CurrentFloorArea:F2} m2.");
    }

    private void CreatePlaceholderUI()
    {
        GameObject canvasObject = new GameObject("Calibration Placeholder UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        dirtGroup = canvasObject.AddComponent<CanvasGroup>();
        dirtGroup.blocksRaycasts = false;
        Image background = CreateImage("Dirt Overlay", canvasObject.transform, new Color(0.18f, 0.12f, 0.06f, 1f));
        Stretch(background.rectTransform);

        for (int i = 0; i < 12; i++)
        {
            Image blotch = CreateImage("Dirt Blotch", canvasObject.transform, new Color(0.08f, 0.055f, 0.025f, 0.8f));
            RectTransform rect = blotch.rectTransform;
            rect.anchorMin = new Vector2(Random.value, Random.value);
            rect.anchorMax = rect.anchorMin;
            rect.sizeDelta = Vector2.one * Random.Range(130f, 310f);
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        messageText = CreateText("Scan Message", canvasObject.transform);
        RectTransform messageRect = messageText.rectTransform;
        messageRect.anchorMin = new Vector2(0.08f, 0.08f);
        messageRect.anchorMax = new Vector2(0.92f, 0.25f);
        messageRect.offsetMin = messageRect.offsetMax = Vector2.zero;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontSize = 42;
        messageText.color = Color.white;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
