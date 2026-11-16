using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    [Header("UI")]
    public Button StartButton;
    public TMP_Text TimerText; // opzionale

    [Header("Impostazioni")]
    public float GameDuration = 120f; // 2 minuti

    private bool isPlaying;
    private bool timerRunning;
    private float timeLeft;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    private void Start()
    {
        if (StartButton != null)
        {
            StartButton.onClick.AddListener(StartGame);
        }

        UpdateTimerText();
    }

    private void Update()
    {
        if (!timerRunning) return;

        timeLeft -= Time.deltaTime;
        UpdateTimerText();

        if (timeLeft <= 0f)
        {
            EndGame();
        }
    }

    // Chiamato dal bottone Start
    public void StartGame()
    {
        if (isPlaying) return;

        isPlaying = true;

        if (StartButton != null)
        {
            StartButton.gameObject.SetActive(false);
        }

        SpawnManager.Singleton.StartInitialSpawn();
    }

    // Chiamato dallo SpawnManager quando i primi sacchetti sono stati raccolti
    public void OnInitialBagsCollected()
    {
        timeLeft = GameDuration;
        timerRunning = true;

        SpawnManager.Singleton.StartTimedSpawning();
    }

    public void EndGame()
    {
        timerRunning = false;
        isPlaying = false;
        timeLeft = 0f;
        UpdateTimerText();

        SpawnManager.Singleton.StopSpawning();

        if (StartButton != null)
        {
            StartButton.gameObject.SetActive(true);
        }

        Debug.Log("Game Over");
    }

    private void UpdateTimerText()
    {
        if (TimerText == null) return;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}