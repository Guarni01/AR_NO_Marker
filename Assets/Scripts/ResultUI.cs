using UnityEngine;
using TMPro;

// A fine countdown scrive in Console se hai vinto o perso (nessuna UI)
public class ResultUI : MonoBehaviour
{
    [Header("UI risultato")]
    public GameObject WinCanvas;
    public GameObject LoseCanvas;
    public TMP_Text TrashLeftText;

    public string TrashTag = "Trash"; // tag dei sacchetti

    private bool wasPlaying;

    private void Start()
    {
        if (WinCanvas != null) WinCanvas.SetActive(false);
        if (LoseCanvas != null) LoseCanvas.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Singleton == null) return;

        bool playing = GameManager.Singleton.IsPlaying;

        // Fine countdown: win se non ci sono sacchetti col tag, lose altrimenti
        if (!playing && wasPlaying)
        {
            int trashLeft = CountTrash();

            if (trashLeft == 0)
            {
                Debug.Log("YOU WIN - nessun sacchetto rimasto");
                if (WinCanvas != null) WinCanvas.SetActive(true);
            }
            else
            {
                Debug.Log("YOU LOSE - sacchetti rimasti: " + trashLeft);
                if (LoseCanvas != null)
                {
                    LoseCanvas.SetActive(true);
                    TrashLeftText.text = trashLeft.ToString();
                } 
            }
        }

        if (playing && !wasPlaying)
{
        if (WinCanvas != null) WinCanvas.SetActive(false);
        if (LoseCanvas != null) LoseCanvas.SetActive(false);
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
}
