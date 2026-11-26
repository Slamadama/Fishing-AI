// Fills the water by cloning the scene's inactive prey and predator templates, one batch per species.
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [System.Serializable]
    private class FishSpecies
    {
        public string speciesName;
        public Sprite sprite;
        public bool aggressive;
        public int count = 2;
        public float length = 2f;
    }

    [Header("Templates")]
    [SerializeField] private GameObject preyTemplate;
    [SerializeField] private GameObject predatorTemplate;

    [Header("Species")]
    [SerializeField] private FishSpecies[] species = new FishSpecies[0];

    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaSize = new Vector2(170f, 170f);
    [SerializeField] private float waterLevel = 20.6f;
    [SerializeField] private float minDepthBelowSurface = 5f;
    [SerializeField] private float maxDepthBelowSurface = 11f;
    [SerializeField] private float bedClearance = 1f;
    [SerializeField] private float bedProbeHeight = 30f;

    private void Start()
    {
        SpawnAll();
    }

    public void SpawnAll()
    {
        if (species == null)
            return;

        foreach (FishSpecies entry in species)
        {
            SpawnSpecies(entry);
        }
    }

    private void SpawnSpecies(FishSpecies entry)
    {
        GameObject template = entry.aggressive ? predatorTemplate : preyTemplate;

        if (template == null)
        {
            Debug.LogWarning("FishSpawner has no " + (entry.aggressive ? "predator" : "prey") + " template.", this);
            return;
        }

        for (int i = 0; i < entry.count; i++)
        {
            if (!TryFindSpawnPoint(out Vector3 point))
            {
                Debug.LogWarning("FishSpawner found no open water for " + entry.speciesName + ".", this);
                continue;
            }

            // Cloned inactive so Awake runs only after the sprite, size and position are final.
            GameObject fish = Instantiate(template, transform);

            fish.name = entry.speciesName;
            fish.transform.SetPositionAndRotation(point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            Dress(fish, entry);

            fish.SetActive(true);
        }
    }

    private void Dress(GameObject fish, FishSpecies entry)
    {
        if (entry.sprite == null || entry.length <= 0f)
            return;

        SpriteRenderer spriteRenderer = fish.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = entry.sprite;

        Vector2 spriteSize = entry.sprite.bounds.size;

        if (spriteSize.x <= 0.0001f)
            return;

        // One uniform scale turns sprite pixels into metres, and the collider then follows the artwork.
        fish.transform.localScale = Vector3.one * (entry.length / spriteSize.x);

        BoxCollider body = fish.GetComponent<BoxCollider>();

        if (body != null)
            body.size = new Vector3(spriteSize.x, spriteSize.y, 0.6f);
    }

    private bool TryFindSpawnPoint(out Vector3 point)
    {
        for (int attempt = 0; attempt < 24; attempt++)
        {
            Vector2 flat = Random.insideUnitCircle;
            Vector3 candidate = transform.position + new Vector3(flat.x * areaSize.x * 0.5f, 0f, flat.y * areaSize.y * 0.5f);

            // Straight down from above the surface: a hit is the lake bed, a miss is dry land or empty space.
            Vector3 probe = new Vector3(candidate.x, waterLevel + bedProbeHeight, candidate.z);

            if (!Physics.Raycast(probe, Vector3.down, out RaycastHit hit, bedProbeHeight + 100f, ~0, QueryTriggerInteraction.Ignore))
                continue;

            float highest = waterLevel - minDepthBelowSurface;
            float lowest = Mathf.Max(hit.point.y + bedClearance, waterLevel - maxDepthBelowSurface);

            if (highest - lowest < 0.4f)
                continue;

            point = new Vector3(candidate.x, Random.Range(lowest, highest), candidate.z);

            return true;
        }

        point = transform.position;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.75f, 1f, 0.5f);
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, waterLevel, transform.position.z),
            new Vector3(areaSize.x, 0.1f, areaSize.y));
    }
}
