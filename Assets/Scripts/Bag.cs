using UnityEngine;

public class Bag : MonoBehaviour
{
    public string PlayerTag = "Player";
    public Collider AreaCollider; // area off limits (se vuoto prende il primo collider trovato)

    private bool collected;

    private void Awake()
    {
        if (AreaCollider == null)
        {
            AreaCollider = GetComponentInChildren<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag(PlayerTag)) return;

        collected = true;
        SpawnManager.Singleton.OnBagCollected(this);
        Destroy(gameObject);
    }
}