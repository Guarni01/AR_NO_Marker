using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Singleton;

    [Header("Prefab")]
    public GameObject BagPrefab;
    public float SpawnHeightOffset = 0.05f;
    public int MaxSpawnAttempts = 30;   // tentativi per trovare un punto libero
    public float EdgeMargin = 0.05f;    // distanza minima dai bordi del pavimento (metri), 0 = nessun margine
    public float AreaScale = 0.5f;      // quanto dell'area off limits conta (1 = intera, 0.5 = metà, 0 = nessun controllo)

    [Header("Spawn iniziale")]
    public int InitialBagsCount = 3;
    public float InitialSpawnDelay = 0.5f; // pausa tra uno spawn e l'altro

    [Header("Spawn a tempo")]
    public int MinSpawnAmount = 1;         // quanti prefab per ondata (min)
    public int MaxSpawnAmount = 3;         // quanti prefab per ondata (max)
    public float MinSpawnInterval = 3f;    // secondi tra le ondate (min)
    public float MaxSpawnInterval = 10f;   // secondi tra le ondate (max)
    public float StopSpawnSecondsBeforeEnd = 5f; // smette di spawnare quando mancano questi secondi alla fine

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

    // duration = durata totale del countdown (la passa il GameManager)
    public void StartTimedSpawning(float duration)
    {
        if (timedSpawnRoutine != null) StopCoroutine(timedSpawnRoutine);
        timedSpawnRoutine = StartCoroutine(TimedSpawnRoutine(duration));
    }

    private IEnumerator TimedSpawnRoutine(float duration)
    {
        // Dopo questo istante non spawna più niente
        float cutoffTime = Time.time + duration - StopSpawnSecondsBeforeEnd;

        while (true)
        {
            float wait = Random.Range(MinSpawnInterval, MaxSpawnInterval);

            // Se la prossima ondata cadrebbe negli ultimi secondi, lo spawn è finito
            if (Time.time + wait >= cutoffTime) yield break;

            yield return new WaitForSeconds(wait);

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

    // ---------- Pavimento (solo lettura da ARManager) ----------

    // Usa il pavimento scelto dalla calibrazione; se non c'è, prende il piano orizzontale più basso
    private PlaneVisibilityBH GetFloor()
    {
        ARManager ar = ARManager.Singleton;

        if (ar == null) return null;
        if (ar.FloorPlane != null) return ar.FloorPlane;

        PlaneVisibilityBH lowest = null;
        float minY = float.MaxValue;

        for (int i = 0; i < ar.CreatedPlanes.Count; i++)
        {
            PlaneVisibilityBH plane = ar.CreatedPlanes[i];

            if (plane == null || !plane.IsHorizontal) continue;

            if (plane.transform.position.y < minY)
            {
                minY = plane.transform.position.y;
                lowest = plane;
            }
        }

        return lowest;
    }

    // ---------- Spawn singolo ----------

    private bool TrySpawnBag()
    {
        if (BagPrefab == null) return false;

        PlaneVisibilityBH floor = GetFloor();

        if (floor == null)
        {
            Debug.LogWarning("SpawnManager: nessun pavimento disponibile.");
            return false;
        }

        int samplerFails = 0;
        int overlapFails = 0;

        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            // Prima con il margine dai bordi, se non trova nulla riprovo senza margine
            if (!FloorSampler.TryGetRandomPoint(floor, EdgeMargin, out Vector3 point) &&
                !FloorSampler.TryGetRandomPoint(floor, 0f, out point))
            {
                samplerFails++;
                continue;
            }

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
                overlapFails++;
                Destroy(go); // troppo vicino, riprovo con un altro punto
                continue;
            }

            activeBags.Add(bag);
            return true;
        }

        Debug.LogWarning("SpawnManager: nessun punto libero dopo " + MaxSpawnAttempts + " tentativi. " +
                         "Punto non trovato sul piano: " + samplerFails + " volte | " +
                         "Troppo vicino ad altri sacchetti: " + overlapFails + " volte.");
        return false;
    }

    // L'area off limits è il collider dentro il prefab: se si sovrappone a quello di un altro sacchetto, scarto il punto
    private bool IsTooCloseToOtherBags(Bag newBag)
    {
        Bounds newBounds = newBag.AreaCollider.bounds;
        newBounds.size *= AreaScale;

        for (int i = 0; i < activeBags.Count; i++)
        {
            if (activeBags[i] == null) continue;

            Bounds otherBounds = activeBags[i].AreaCollider.bounds;
            otherBounds.size *= AreaScale;

            if (newBounds.Intersects(otherBounds))
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


// Restituisce punti casuali dentro il contorno reale di un piano AR,
// tenendosi a una distanza minima dai bordi.
public static class FloorSampler
{
    public static bool TryGetRandomPoint(PlaneVisibilityBH plane, float edgeMargin, out Vector3 point)
    {
        point = Vector3.zero;

        if (plane == null) return false;

        // PlaneVisibilityBH è sul prefab AR Default Plane, insieme (o sotto) ad ARPlane
        ARPlane arPlane = plane.GetComponentInParent<ARPlane>();
        if (arPlane == null) return false;

        NativeArray<Vector2> boundary = arPlane.boundary;
        if (boundary.Length < 3) return false;

        // Bounding box del contorno (coordinate locali del piano: x = x, y = z)
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        for (int i = 0; i < boundary.Length; i++)
        {
            minX = Mathf.Min(minX, boundary[i].x);
            maxX = Mathf.Max(maxX, boundary[i].x);
            minZ = Mathf.Min(minZ, boundary[i].y);
            maxZ = Mathf.Max(maxZ, boundary[i].y);
        }

        // Estraggo punti a caso nel box finché uno cade dentro al contorno, lontano dai bordi
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 p = new Vector2(Random.Range(minX, maxX), Random.Range(minZ, maxZ));

            if (!IsInside(boundary, p)) continue;
            if (DistanceToEdge(boundary, p) < edgeMargin) continue;

            point = arPlane.transform.TransformPoint(new Vector3(p.x, 0f, p.y));
            return true;
        }

        return false;
    }

    private static bool IsInside(NativeArray<Vector2> poly, Vector2 p)
    {
        bool inside = false;
        int n = poly.Length;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[j];

            if ((a.y > p.y) != (b.y > p.y) &&
                p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static float DistanceToEdge(NativeArray<Vector2> poly, Vector2 p)
    {
        float min = float.MaxValue;
        int n = poly.Length;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 a = poly[j];
            Vector2 b = poly[i];
            Vector2 ab = b - a;

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.000001f));
            float d = Vector2.Distance(p, a + ab * t);

            if (d < min) min = d;
        }

        return min;
    }
}
