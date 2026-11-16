using UnityEngine;
using TMPro;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Singleton;

    [Header("Testi")]
    public string WinText = "YOU WIN";
    public string LoseText = "YOU LOSE";

    [Header("Rilevamento")]
    public string TrashTag = "Trash"; // tag dei sacchetti

    [Header("Aspetto")]
    public int FontSize = 120;
    public Color WinColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    public Color LoseColor = new Color(0.95f, 0.2f, 0.2f, 1f);
    public float TopMargin = 400f; // distanza dal centro verso l'alto

    private GameObject canvasObject;
    private TextMeshProUGUI label;
    private bool wasPlaying;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;

        BuildUI();
        canvasObject.SetActive(false); // nascosto finché non finisce il countdown
    }

    private void Update()
    {
        if (GameManager.Singleton == null) return;

        bool playing = GameManager.Singleton.IsPlaying;

        // Appena premi Start: nascondo il risultato precedente
        if (playing && !wasPlaying)
        {
            Hide();
        }

        // Fine countdown: win se non ci sono sacchetti col tag, lose altrimenti
        if (!playing && wasPlaying)
        {
            Show(CountTrash() == 0);
        }

        wasPlaying = playing;
    }

    private int CountTrash()
    {
        try
        {
            return GameObject.FindGameObjectsWithTag(TrashTag).Length;
        }
        catch (UnityException)
        {
            Debug.LogError("ResultUI: il tag '" + TrashTag + "' non esiste. Crealo in Tags & Layers e assegnalo al prefab.");
            return 0;
        }
    }

    public void Show(bool won)
    {
        label.text = won ? WinText : LoseText;
        label.color = won ? WinColor : LoseColor;
        canvasObject.SetActive(true);
    }

    public void Hide()
    {
        canvasObject.SetActive(false);
    }

    private void BuildUI()
    {
        canvasObject = new GameObject("ResultCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        // Nessun GraphicRaycaster: il testo non blocca i tocchi

        GameObject labelObject = new GameObject("ResultLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, TopMargin);
        rect.sizeDelta = new Vector2(1000f, 250f);

        label = labelObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = FontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }
}
