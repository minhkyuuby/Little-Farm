using UnityEngine;

public class DeliveryGuyController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField] private ProductPackage _productPackage;

    public ProductPackage ProductPackage => _productPackage;
    public bool HasCargo => _productPackage != null && _productPackage.HasFruits;
    public bool IsCargoEmpty => _productPackage == null || _productPackage.IsEmpty;

    void OnValidate()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (_productPackage == null)
        {
            _productPackage = GetComponentInChildren<ProductPackage>();
        }
    }
    
    public bool TryHarvestFromPlant(Plant plant)
    {
        if (plant == null || _productPackage == null || !_productPackage.IsEmpty)
        {
            return false;
        }

        return plant.TryHarvest(_productPackage);
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
}
