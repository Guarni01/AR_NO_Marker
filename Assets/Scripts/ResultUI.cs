using UnityEngine;

// A fine countdown scrive in Console se hai vinto o perso (nessuna UI)
public class ResultUI : MonoBehaviour
{
    public string TrashTag = "Trash"; // tag dei sacchetti

    private bool wasPlaying;

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
            }
            else
            {
                Debug.Log("YOU LOSE - sacchetti rimasti: " + trashLeft);
            }
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
