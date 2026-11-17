using UnityEngine;

public class Bag : MonoBehaviour
{
    public string PlayerTag = "Player";
    public Collider AreaCollider;

    [SerializeField] private ParticleSystem destroyEffect;

    private bool collected;

    private void Awake()
    {
        if (AreaCollider == null)
            AreaCollider = GetComponentInChildren<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag(PlayerTag)) return;

        CollectBag();
    }

    public void Collect()
    {
        if (collected) return;

        CollectBag();
    }

    private void CollectBag()
    {
        collected = true;

        // Crea le particelle dove si trova il sacco
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, transform.rotation);

        SpawnManager.Singleton.OnBagCollected(this);
        Destroy(gameObject);
    }
}