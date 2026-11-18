using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    [Header("UI")]
    public TMP_Text TimerText;

    [Header("Impostazioni")]
    public float GameDuration = 120f; // 2 minuti

    private bool isPlaying;
    private bool timerRunning;
    private float timeLeft;

    // Sola lettura: true da quando premi Start fino a fine countdown (usato da ResultUI)
    public bool IsPlaying => isPlaying;

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

    // Chiamato da StartButtonUI quando premi il bottone
    public void StartGame()
    {
        if (isPlaying) return;

        isPlaying = true;
        SpawnManager.Singleton.StartInitialSpawn();
    }

    // Chiamato dallo SpawnManager quando i primi sacchetti sono stati raccolti
    public void OnInitialBagsCollected()
    {
        timeLeft = GameDuration;
        timerRunning = true;

        SpawnManager.Singleton.StartTimedSpawning(GameDuration);
    }

    public void EndGame()
    {
        timerRunning = false;
        isPlaying = false;
        timeLeft = 0f;
        UpdateTimerText();

        SpawnManager.Singleton.StopSpawning();

        // Rimostra il bottone per poter rigiocare
        if (StartButtonUI.Singleton != null)
        {
            StartButtonUI.Singleton.OnCalibrationFinished();
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
