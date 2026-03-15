using UnityEngine;
using Unity.Behavior;
using LittleFarm.GameplayEventSubject;

public class DeliveryGuyController : MonoBehaviour, IPoolable
{
    [SerializeField] private ProductPackage _productPackage;
    [SerializeField] private BehaviorGraphAgent _behaviorRunner;
    [SerializeField] private bool _forceRestartBehaviorOnSpawn = true;

    private Plant _targetPlant;
    private Dock _targetDock;
    private Vector3 _releasePosition;

    public ProductPackage ProductPackage => _productPackage;
    public bool HasCargo => _productPackage != null && _productPackage.HasFruits;
    public bool IsCargoEmpty => _productPackage == null || _productPackage.IsEmpty;
    public Plant TargetPlant => _targetPlant;
    public Dock TargetDock => _targetDock;
    public Vector3 ReleasePosition => _releasePosition;

    void OnValidate()
    {
        if (_productPackage == null)
        {
            _productPackage = GetComponentInChildren<ProductPackage>();
        }

        if (_behaviorRunner == null)
        {
            _behaviorRunner = GetComponent<BehaviorGraphAgent>();
        }
    }
    
    public bool TryHarvestFromPlant(Plant plant)
    {
        if (plant == null || _productPackage == null || !_productPackage.IsEmpty)
        {
            return false;
        }

        var harvested = plant.TryHarvest(_productPackage);
        if (harvested)
        {
            _productPackage.SetPackagePrice(plant.CurrentFruitBasePriceCoin);
            _targetPlant = null;
        }

        return harvested;
    }

    public bool TryDeliverToCustomer(CustomerController customer)
    {
        if (customer == null || _productPackage == null || !_productPackage.HasFruits)
        {
            return false;
        }

        return customer.TryReceiveDelivery(_productPackage);
    }

    public bool TryProcessInteraction(Plant plant, CustomerController customer)
    {
        if (plant != null && IsCargoEmpty)
        {
            return TryHarvestFromPlant(plant);
        }

        if (customer != null && HasCargo)
        {
            return TryDeliverToCustomer(customer);
        }

        return false;
    }

    public void SetTargetPlant(Plant plant)
    {
        _targetPlant = plant;
    }

    public void SetTargetDock(Dock dock)
    {
        _targetDock = dock;
    }

    public void SetReleasePosition(Vector3 releasePosition)
    {
        _releasePosition = releasePosition;
    }

    public bool TryReserveDeliveryDock(Market market = null)
    {
        market ??= Market.Instance;

        if (market == null || !HasCargo)
        {
            return false;
        }

        if (market.TryReserveDockForDelivery(this, out var dock))
        {
            _targetDock = dock;
            return true;
        }

        return false;
    }

    public bool TryDeliverAtDock(Dock dock = null)
    {
        var targetDock = dock != null ? dock : _targetDock;
        if (targetDock == null)
        {
            return false;
        }

        return targetDock.TryCompleteDelivery(this);
    }

    public void ReturnToPool()
    {
        var poolManager = Object.FindFirstObjectByType<PoolManager>();
        if (poolManager != null)
        {
            poolManager.Release(this);
            return;
        }

        gameObject.SetActive(false);
    }

    public void OnSpawned()
    {
        _targetDock = null;

        if (_forceRestartBehaviorOnSpawn)
        {
            ForceRestartBehaviorRunner();
        }
    }

    public void OnDespawned()
    {
        _targetDock = null;
        EventBus.Publish(new DeliveryGuyReturned(this));
    }

    private void ForceRestartBehaviorRunner()
    {
        if (_behaviorRunner == null)
        {
            _behaviorRunner = GetComponent<BehaviorGraphAgent>();
        }

        if (_behaviorRunner == null)
        {
            return;
        }

        _behaviorRunner.Restart();
    }
}
