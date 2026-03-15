using System;
using System.Collections.Generic;
using UnityEngine;
using LittleFarm.GameplayEventSubject;
using LittleFarm.UpgradesEventSubject;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private CustomerController _customerPrefab;
    [SerializeField] private Market _market;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _releaseEndPoint;
    [SerializeField] private string _poolId = "customers";
    [SerializeField] private PoolSettings _poolSettings = new();
    [SerializeField, Min(1)] private int _maxActiveCustomers = 1;
    [SerializeField] private ManagerUpgradeSystem _upgradeSystem;

    private readonly List<CustomerController> _activeCustomers = new();
    private int _defaultMaxActiveCustomers = 1;

    public int MaxActiveCustomers => Mathf.Max(1, _maxActiveCustomers);
    public event Action<int> OnMaxActiveCustomersChanged;

    private void OnValidate()
    {
        if (_market == null)
        {
            _market = FindFirstObjectByType<Market>() ?? Market.Instance;
        }

        if (_upgradeSystem == null)
        {
            _upgradeSystem = ManagerUpgradeSystem.Instance ?? FindFirstObjectByType<ManagerUpgradeSystem>();
        }
    }

    private void Start()
    {
        _market ??= FindFirstObjectByType<Market>() ?? Market.Instance;
        _defaultMaxActiveCustomers = Mathf.Max(1, _maxActiveCustomers);
        TryBindUpgradeSystem();
        ApplyCapacityFromUpgrades();
        EnsurePool();
        TryFillCapacity();
    }

    private void OnEnable()
    {
        TryBindUpgradeSystem();
        EventBus.Subscribe<DockBecameEmpty>(OnDockBecameEmpty);
        EventBus.Subscribe<UpgradePaid>(OnUpgradePaid);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DockBecameEmpty>(OnDockBecameEmpty);
        EventBus.Unsubscribe<UpgradePaid>(OnUpgradePaid);
    }

    private void Update()
    {
        CleanupInactive();
        TryFillCapacity();
    }

    public void SetMaxActiveCustomers(int maxActive)
    {
        var clamped = Mathf.Max(1, maxActive);
        if (_maxActiveCustomers == clamped)
        {
            return;
        }

        _maxActiveCustomers = clamped;
        CleanupInactive();
        TryFillCapacity();
        OnMaxActiveCustomersChanged?.Invoke(_maxActiveCustomers);
    }

    public void IncreaseMaxActiveCustomers(int amount = 1)
    {
        SetMaxActiveCustomers(_maxActiveCustomers + Mathf.Max(0, amount));
    }

    public void ApplyCapacityFromUpgrades()
    {
        var bonus = _upgradeSystem != null ? _upgradeSystem.CustomerCapacityBonus : 0;
        var targetCapacity = Mathf.Max(1, _defaultMaxActiveCustomers + Mathf.Max(0, bonus));
        SetMaxActiveCustomers(targetCapacity);
    }

    private void EnsurePool()
    {
        if (_customerPrefab == null)
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
            poolManager.CreatePool(_poolId, _customerPrefab, _poolSettings);
        }
    }

    private void CleanupInactive()
    {
        for (var i = _activeCustomers.Count - 1; i >= 0; i--)
        {
            var customer = _activeCustomers[i];
            if (customer == null || !customer.gameObject.activeInHierarchy)
            {
                _activeCustomers.RemoveAt(i);
            }
        }
    }

    private bool TrySpawnCustomer()
    {
        _market ??= FindFirstObjectByType<Market>() ?? Market.Instance;

        if (_activeCustomers.Count >= MaxActiveCustomers)
        {
            return false;
        }

        if (_market == null || _customerPrefab == null)
        {
            return false;
        }

        if (!_market.TryGetEmptyDock(out _))
        {
            return false;
        }

        var spawnRef = _spawnPoint != null ? _spawnPoint : transform;
        var releaseRef = _releaseEndPoint != null ? _releaseEndPoint : spawnRef;
        var poolManager = PoolManager.Instance;
        var customer = poolManager != null
            ? poolManager.Spawn<CustomerController>(_poolId, spawnRef.position, spawnRef.rotation)
            : null;

        if (customer == null)
        {
            return false;
        }

        customer.ResetPurchaseState();
        if (!customer.TryAssignToDock(_market))
        {
            customer.ReturnToPool();
            return false;
        }

        customer.SetReleasePosition(releaseRef.position);

        _activeCustomers.Add(customer);
        return true;
    }

    private void TryFillCapacity()
    {
        while (_activeCustomers.Count < MaxActiveCustomers)
        {
            if (!TrySpawnCustomer())
            {
                break;
            }
        }
    }

    private void OnDockBecameEmpty(DockBecameEmpty message)
    {
        _market ??= FindFirstObjectByType<Market>() ?? Market.Instance;

        CleanupInactive();

        if (_activeCustomers.Count >= MaxActiveCustomers)
        {
            return;
        }

        if (_market == null || _customerPrefab == null || message.Dock == null)
        {
            return;
        }

        if (!message.Dock.IsEmpty)
        {
            return;
        }

        var spawnRef = _spawnPoint != null ? _spawnPoint : transform;
        var releaseRef = _releaseEndPoint != null ? _releaseEndPoint : spawnRef;
        var poolManager = PoolManager.Instance;
        var customer = poolManager != null
            ? poolManager.Spawn<CustomerController>(_poolId, spawnRef.position, spawnRef.rotation)
            : null;

        if (customer == null)
        {
            return;
        }

        customer.ResetPurchaseState();
        if (!message.Dock.TryAssignCustomer(customer))
        {
            customer.ReturnToPool();
            return;
        }

        customer.SetTargetMarket(_market);
        customer.SetReleasePosition(releaseRef.position);

        _activeCustomers.Add(customer);
        TryFillCapacity();
    }

    private void TryBindUpgradeSystem()
    {
        var resolved = _upgradeSystem != null ? _upgradeSystem : ManagerUpgradeSystem.Instance ?? FindFirstObjectByType<ManagerUpgradeSystem>();
        if (resolved == _upgradeSystem)
        {
            return;
        }

        _upgradeSystem = resolved;
    }

    private void OnUpgradePaid(UpgradePaid message)
    {
        ApplyCapacityFromUpgrades();
    }
}
