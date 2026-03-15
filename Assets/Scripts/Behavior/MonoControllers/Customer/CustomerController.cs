using UnityEngine;
using Unity.Behavior;

public class CustomerController : MonoBehaviour, IPoolable
{
	[SerializeField] private ProductPackage _productPackage;
	[SerializeField] private BehaviorGraphAgent _behaviorRunner;
	[SerializeField] private bool _forceRestartBehaviorOnSpawn = true;
	private Dock _targetDock;
	private Market _targetMarket;
	private Vector3 _releasePosition;

	private int _purchasedFruitCount;
	private bool _hasPurchased;

	public bool HasPurchased => _hasPurchased;
	public int PurchasedFruitCount => _purchasedFruitCount;
	public ProductPackage ProductPackage => _productPackage;
	public Dock TargetDock => _targetDock;
	public Market TargetMarket => _targetMarket;
	public Vector3 ReleasePosition => _releasePosition;

	private void OnValidate()
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

	public bool TryReceiveDelivery(ProductPackage sourcePackage)
	{
		if (_productPackage == null || sourcePackage == null)
		{
			return false;
		}

		return _productPackage.TryReceiveFrom(sourcePackage);
	}

	public bool TryPurchaseFromOwnPackage()
	{
		if (_productPackage == null || _productPackage.IsEmpty)
		{
			return false;
		}

		var purchasedNow = _productPackage.Count;
		AcceptPurchase(purchasedNow);
		return purchasedNow > 0;
	}

	public void AcceptPurchase(int fruitCount)
	{
		if (fruitCount <= 0)
		{
			return;
		}

		_purchasedFruitCount += fruitCount;
		_hasPurchased = true;
	}

	public void ResetPurchaseState()
	{
		_purchasedFruitCount = 0;
		_hasPurchased = false;
	}

	public void SetTargetDock(Dock dock)
	{
		_targetDock = dock;
	}

	public void SetTargetMarket(Market market)
	{
		_targetMarket = market;
	}

	public void SetReleasePosition(Vector3 releasePosition)
	{
		_releasePosition = releasePosition;
	}

	public bool TryAssignToDock(Market market)
	{
		market ??= Market.Instance;

		if (market == null)
		{
			return false;
		}

		if (market.TryAssignCustomerToDock(this, out var dock))
		{
			_targetDock = dock;
			_targetMarket = market;
			return true;
		}

		return false;
	}

	public bool TryBuyAtTargetDock(Dock dock = null)
	{
		var targetDock = dock != null ? dock : _targetDock;
		if (targetDock == null)
		{
			return false;
		}

		return targetDock.TryCompleteCustomerPurchase(this);
	}

	public void ReturnToPool()
	{
		if (_productPackage != null && !_productPackage.IsEmpty)
		{
			_productPackage.ReleaseAllFruitsToPool();
		}

		var poolManager = Object.FindFirstObjectByType<PoolManager>();
		if (poolManager != null)
		{
			poolManager.Release(this);
			return;
		}

		gameObject.SetActive(false);
	}

	public void Despawn()
	{
		_targetDock = null;
		_targetMarket = null;
		ReturnToPool();
	}

	public void OnSpawned()
	{
		_targetDock = null;
		_targetMarket = null;
		ResetPurchaseState();

		if (_forceRestartBehaviorOnSpawn)
		{
			ForceRestartBehaviorRunner();
		}
	}

	public void OnDespawned()
	{
		_targetDock = null;
		_targetMarket = null;
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
