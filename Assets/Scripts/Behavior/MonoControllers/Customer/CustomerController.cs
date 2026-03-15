using UnityEngine;

public class CustomerController : MonoBehaviour
{
	[SerializeField] private ProductPackage _productPackage;

	private int _purchasedFruitCount;
	private bool _hasPurchased;

	public bool HasPurchased => _hasPurchased;
	public int PurchasedFruitCount => _purchasedFruitCount;
	public ProductPackage ProductPackage => _productPackage;

	private void OnValidate()
	{
		if (_productPackage == null)
		{
			_productPackage = GetComponentInChildren<ProductPackage>();
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

		var purchasedNow = _productPackage.ReleaseAllFruitsToPool();
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

	public void Despawn()
	{
		gameObject.SetActive(false);
	}
}
