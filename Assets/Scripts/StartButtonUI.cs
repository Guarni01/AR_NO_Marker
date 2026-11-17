using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StartButtonUI : MonoBehaviour
{
    public static StartButtonUI Singleton;

    [Header("UI esistente")]
    public GameObject CanvasButton;

    [Header("Aspetto bottone")]
    public string ButtonLabel = "START";
    public Vector2 ButtonSize = new Vector2(500f, 150f);
    public float BottomMargin = 150f;
    public int FontSize = 60;
    public Color ButtonColor = new Color(0.1f, 0.6f, 0.2f, 1f);

    [Header("Calibrazione (opzionale)")]
    // Se attivo, il bottone compare da solo quando ARManager ha almeno MinPlanesForCalibration piani
    public bool AutoShowWhenPlanesFound = false;
    public int MinPlanesForCalibration = 1;

    private GameObject buttonObject;
    private bool autoShown;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;

        CreateEventSystemIfMissing();
        //BuildButton();
        buttonObject = CanvasButton;
        buttonObject.GetComponent<Button>().onClick.AddListener(OnStartClicked);
        
        buttonObject.SetActive(false); // nascosto finché la calibrazione non è finita
    }

    private void Update()
    {
        if (!AutoShowWhenPlanesFound || autoShown) return;
        if (ARManager.Singleton == null) return;

        if (ARManager.Singleton.CreatedPlanes.Count >= MinPlanesForCalibration)
        {
            autoShown = true;
            OnCalibrationFinished();
        }
    }

    // Da richiamare quando la calibrazione è terminata
    public void OnCalibrationFinished()
    {
        buttonObject.SetActive(true);
    }

    // Per nasconderlo / rimostrarlo a fine partita
    public void Hide()
    {
        buttonObject.SetActive(false);
    }

    private void OnStartClicked()
    {
        Hide();

        if (GameManager.Singleton != null)
        {
            GameManager.Singleton.StartGame();
        }
        else
        {
            Debug.LogError("StartButtonUI: nessun GameManager in scena!");
        }
    }

    // ---------- Creazione UI da codice ----------

    /*private void BuildButton()
    {
        // Canvas
        GameObject canvasObject = new GameObject("StartCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // Bottone
        buttonObject = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, BottomMargin);
        rect.sizeDelta = ButtonSize;

        Image image = buttonObject.GetComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnStartClicked);

        // Testo
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = ButtonLabel;
        label.fontSize = FontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }*/

    private void CreateEventSystemIfMissing()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
