using System.Collections.Generic;
using UnityEngine;
using LittleFarm.GameplayEventSubject;

public class DeliveryGuySpawner : MonoBehaviour
{
    [SerializeField] private DeliveryGuyController _deliveryGuyPrefab;
    [SerializeField] private Garden _garden;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _releaseEndPoint;
    [SerializeField] private string _poolId = "delivery-guys";
    [SerializeField] private PoolSettings _poolSettings = new();
    [SerializeField, Min(1)] private int _maxActiveDeliveryGuys = 1;

    private readonly List<DeliveryGuyController> _activeDeliveryGuys = new();

    public int MaxActiveDeliveryGuys => Mathf.Max(1, _maxActiveDeliveryGuys);

    private void OnValidate()
    {
        if (_garden == null)
        {
            _garden = FindFirstObjectByType<Garden>();
        }
    }

    private void Start()
    {
        EnsurePool();
        TryFillCapacity();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlantProduced>(OnPlantProduced);
        EventBus.Subscribe<DeliveryGuyReturned>(OnDeliveryGuyReturned);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlantProduced>(OnPlantProduced);
        EventBus.Unsubscribe<DeliveryGuyReturned>(OnDeliveryGuyReturned);
    }

    private void Update()
    {
        CleanupInactive();
        TryFillCapacity();
    }

    public void SetMaxActiveDeliveryGuys(int maxActive)
    {
        _maxActiveDeliveryGuys = Mathf.Max(1, maxActive);
        CleanupInactive();
        TryFillCapacity();
    }

    public void IncreaseMaxActiveDeliveryGuys(int amount = 1)
    {
        SetMaxActiveDeliveryGuys(_maxActiveDeliveryGuys + Mathf.Max(0, amount));
    }

    private void EnsurePool()
    {
        if (_deliveryGuyPrefab == null)
        {
            return;
        }

        var poolManager = PoolManager.Instance;
        if (poolManager == null)
        {
            return;
        }

        if (!poolManager.HasPool(_poolId))
        {
            poolManager.CreatePool(_poolId, _deliveryGuyPrefab, _poolSettings);
        }
    }

    private void CleanupInactive()
    {
        for (var i = _activeDeliveryGuys.Count - 1; i >= 0; i--)
        {
            var deliveryGuy = _activeDeliveryGuys[i];
            if (deliveryGuy == null || !deliveryGuy.gameObject.activeInHierarchy)
            {
                _activeDeliveryGuys.RemoveAt(i);
            }
        }
    }

    private bool TrySpawnDeliveryGuy()
    {
        if (_activeDeliveryGuys.Count >= MaxActiveDeliveryGuys)
        {
            return false;
        }

        if (_garden == null || _deliveryGuyPrefab == null)
        {
            return false;
        }

        if (!_garden.TryGetHarvestablePlant(out var targetPlant, true))
        {
            return false;
        }

        var spawnRef = _spawnPoint != null ? _spawnPoint : transform;
        var releaseRef = _releaseEndPoint != null ? _releaseEndPoint : spawnRef;
        var poolManager = PoolManager.Instance;
        var deliveryGuy = poolManager != null
            ? poolManager.Spawn<DeliveryGuyController>(_poolId, spawnRef.position, spawnRef.rotation)
            : null;

        if (deliveryGuy == null)
        {
            targetPlant.ReleaseHarvestReservation();
            return false;
        }

        deliveryGuy.SetTargetPlant(targetPlant);
        deliveryGuy.SetTargetDock(null);
        deliveryGuy.SetReleasePosition(releaseRef.position);
        _activeDeliveryGuys.Add(deliveryGuy);
        return true;
    }

    private void TryFillCapacity()
    {
        while (_activeDeliveryGuys.Count < MaxActiveDeliveryGuys)
        {
            if (!TrySpawnDeliveryGuy())
            {
                break;
            }
        }
    }

    private void OnPlantProduced(PlantProduced message)
    {
        CleanupInactive();
        TryFillCapacity();
    }

    private void OnDeliveryGuyReturned(DeliveryGuyReturned message)
    {
        CleanupInactive();
        TryFillCapacity();
    }
}
