using System.Collections.Generic;
using UnityEngine;
using LittleFarm.GameplayEventSubject;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private CustomerController _customerPrefab;
    [SerializeField] private Market _market;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private string _poolId = "customers";
    [SerializeField] private PoolSettings _poolSettings = new();
    [SerializeField, Min(1)] private int _maxActiveCustomers = 1;

    private readonly List<CustomerController> _activeCustomers = new();

    public int MaxActiveCustomers => Mathf.Max(1, _maxActiveCustomers);

    private void OnValidate()
    {
        if (_market == null)
        {
            _market = FindFirstObjectByType<Market>() ?? Market.Instance;
        }
    }

    private void Start()
    {
        _market ??= FindFirstObjectByType<Market>() ?? Market.Instance;
        EnsurePool();
        TryFillCapacity();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<DockBecameEmpty>(OnDockBecameEmpty);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DockBecameEmpty>(OnDockBecameEmpty);
    }

    private void Update()
    {
        CleanupInactive();
        TryFillCapacity();
    }

    public void SetMaxActiveCustomers(int maxActive)
    {
        _maxActiveCustomers = Mathf.Max(1, maxActive);
        CleanupInactive();
        TryFillCapacity();
    }

    public void IncreaseMaxActiveCustomers(int amount = 1)
    {
        SetMaxActiveCustomers(_maxActiveCustomers + Mathf.Max(0, amount));
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

        _activeCustomers.Add(customer);
        TryFillCapacity();
    }
}
