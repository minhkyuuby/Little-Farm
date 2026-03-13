using UnityEngine;

public class Garden : MonoBehaviour
{
    [SerializeField] private Transform _plantPrefab;
    [SerializeField] private int _initialPoolSize = 20;
    [SerializeField] private int _maxPoolSize = 250;
    [SerializeField] private Vector3 _spawnArea = new(8f, 0f, 8f);

    private const string PlantPoolId = "plants";

    private void Start()
    {
        if (_plantPrefab == null)
        {
            Debug.LogWarning("Garden: Plant prefab is not assigned.");
            return;
        }

        if (!PoolManager.Instance.HasPool(PlantPoolId))
        {
            var settings = new PoolSettings
            {
                InitialSize = Mathf.Max(0, _initialPoolSize),
                MaxSize = Mathf.Max(1, _maxPoolSize),
                AutoExpand = true
            };

            PoolManager.Instance.CreatePool(PlantPoolId, _plantPrefab, settings);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPlant();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveNearestPlant();
        }
    }

    private void SpawnPlant()
    {
        var randomOffset = new Vector3(
            Random.Range(-_spawnArea.x, _spawnArea.x),
            Random.Range(-_spawnArea.y, _spawnArea.y),
            Random.Range(-_spawnArea.z, _spawnArea.z));

        var spawnPosition = transform.position + randomOffset;
        var plant = PoolManager.Instance.Spawn(_plantPrefab, spawnPosition, Quaternion.identity);

        if (plant == null)
        {
            Debug.LogWarning("Garden: pool exhausted or unavailable.");
        }
    }

    private void RemoveNearestPlant()
    {
        var allPlants = FindObjectsByType<Transform>(FindObjectsSortMode.None);

        Transform nearest = null;
        var nearestDistance = float.MaxValue;
        var origin = transform.position;

        foreach (var candidate in allPlants)
        {
            if (candidate == null || candidate == _plantPrefab || !candidate.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (candidate.name.Replace("(Clone)", string.Empty).Trim() != _plantPrefab.name)
            {
                continue;
            }

            var dist = (candidate.position - origin).sqrMagnitude;
            if (dist < nearestDistance)
            {
                nearest = candidate;
                nearestDistance = dist;
            }
        }

        if (nearest != null)
        {
            PoolManager.Instance.Release(nearest);
        }
    }
}
