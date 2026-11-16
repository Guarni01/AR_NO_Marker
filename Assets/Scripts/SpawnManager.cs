using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Singleton;

    [Header("Prefab")]
    public GameObject BagPrefab;
    public float SpawnHeightOffset = 0.05f;
    public int MaxSpawnAttempts = 30; // tentativi per trovare un punto non troppo vicino agli altri

    [Header("Spawn iniziale")]
    public int InitialBagsCount = 3;
    public float InitialSpawnDelay = 0.5f; // pausa tra uno spawn e l'altro

    [Header("Spawn a tempo")]
    public int MinSpawnAmount = 1;   // quanti prefab per ondata (min)
    public int MaxSpawnAmount = 3;   // quanti prefab per ondata (max)
    public float MinSpawnInterval = 3f;  // secondi tra le ondate (min)
    public float MaxSpawnInterval = 10f; // secondi tra le ondate (max)

    private List<Bag> activeBags = new List<Bag>();
    private int initialBagsCollected;
    private bool isInitialPhase;
    private Coroutine timedSpawnRoutine;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    // ---------- FASE 1: spawn dei 3 sacchetti iniziali ----------

    public void StartInitialSpawn()
    {
        StopSpawning();
        ClearBags();

        initialBagsCollected = 0;
        isInitialPhase = true;

        StartCoroutine(InitialSpawnRoutine());
    }

    private IEnumerator InitialSpawnRoutine()
    {
        for (int i = 0; i < InitialBagsCount; i++)
        {
            TrySpawnBag();
            yield return new WaitForSeconds(InitialSpawnDelay);
        }
    }

    // ---------- FASE 2: spawn a tempo ----------

    public void StartTimedSpawning()
    {
        if (timedSpawnRoutine != null) StopCoroutine(timedSpawnRoutine);
        timedSpawnRoutine = StartCoroutine(TimedSpawnRoutine());
    }

    private IEnumerator TimedSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(MinSpawnInterval, MaxSpawnInterval));

            int amount = Random.Range(MinSpawnAmount, MaxSpawnAmount + 1); // +1 perché il max int è escluso

            for (int i = 0; i < amount; i++)
            {
                TrySpawnBag();
            }
        }
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
        timedSpawnRoutine = null;
    }

    // ---------- Spawn singolo ----------

    private bool TrySpawnBag()
    {
        if (BagPrefab == null || ARManager2.Singleton == null) return false;

        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            if (!ARManager2.Singleton.TryGetRandomPointOnFloor(out Vector3 point)) return false;

            GameObject go = Instantiate(BagPrefab, point + Vector3.up * SpawnHeightOffset, Quaternion.identity);
            Physics.SyncTransforms(); // aggiorna i bounds del collider appena spostato

            Bag bag = go.GetComponent<Bag>();

            if (bag == null || bag.AreaCollider == null)
            {
                Debug.LogError("Il prefab deve avere lo script Bag e un Collider (area off limits)!");
                Destroy(go);
                return false;
            }

            if (IsTooCloseToOtherBags(bag))
            {
                Destroy(go); // troppo vicino, riprovo con un altro punto
                continue;
            }

            activeBags.Add(bag);
            return true;
        }

        Debug.LogWarning("SpawnManager: nessun punto libero trovato dopo " + MaxSpawnAttempts + " tentativi.");
        return false;
    }

    // L'area off limits è il collider dentro il prefab: se si sovrappone a quello di un altro sacchetto, scarto il punto
    private bool IsTooCloseToOtherBags(Bag newBag)
    {
        Bounds newBounds = newBag.AreaCollider.bounds;

        for (int i = 0; i < activeBags.Count; i++)
        {
            if (activeBags[i] == null) continue;

            if (newBounds.Intersects(activeBags[i].AreaCollider.bounds))
            {
                return true;
            }
        }

        return false;
    }

    // ---------- Raccolta ----------

    // Chiamato da Bag quando il giocatore lo raccoglie
    public void OnBagCollected(Bag bag)
    {
        activeBags.Remove(bag);

        if (isInitialPhase)
        {
            initialBagsCollected++;

            if (initialBagsCollected >= InitialBagsCount)
            {
                isInitialPhase = false;
                GameManager.Singleton.OnInitialBagsCollected();
            }
        }
    }

    private void ClearBags()
    {
        for (int i = 0; i < activeBags.Count; i++)
        {
            if (activeBags[i] != null) Destroy(activeBags[i].gameObject);
        }

        activeBags.Clear();
    }
}