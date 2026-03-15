using UnityEngine;
using LittleFarm.GameplayEventSubject;

public enum DockState
{
    Empty = 0,
    CustomerIncoming = 1,
    Available = 2,
    Delivering = 3,
    WaitingToBuy = 4,
}

public class Dock : MonoBehaviour
{
    [SerializeField] private Transform _deliveryPosition;
    [SerializeField] private Transform _customerPosition;
    [SerializeField] private ParticleSystem _purchaseParticleSystem;
    [SerializeField] private DockState _state = DockState.Empty;

    private CustomerController _waitingCustomer;
    private DeliveryGuyController _deliveryGuy;

    public Transform DeliveryPosition => _deliveryPosition != null ? _deliveryPosition : transform;
    public Transform CustomerPosition => _customerPosition != null ? _customerPosition : transform;
    public DockState State => _state;
    public bool IsEmpty => _state == DockState.Empty;
    public bool IsCustomerIncoming => _state == DockState.CustomerIncoming;
    public bool IsAvailable => _state == DockState.Available;
    public bool IsDelivering => _state == DockState.Delivering;
    public bool IsWaitingToBuy => _state == DockState.WaitingToBuy;
    public CustomerController WaitingCustomer => _waitingCustomer;
    public DeliveryGuyController DeliveryGuy => _deliveryGuy;

    public bool TryAssignCustomer(CustomerController customer)
    {
        if (customer == null || !IsEmpty)
        {
            return false;
        }

        _waitingCustomer = customer;
        _deliveryGuy = null;
        _state = DockState.CustomerIncoming;
        customer.SetTargetDock(this);
        return true;
    }

    public bool TryMarkCustomerArrived(CustomerController customer)
    {
        if (customer == null || _waitingCustomer != customer || !IsCustomerIncoming)
        {
            return false;
        }

        _state = DockState.Available;
        return true;
    }

    public bool TryMarkDelivering(DeliveryGuyController deliveryGuy)
    {
        if (deliveryGuy == null || !IsAvailable || _waitingCustomer == null)
        {
            return false;
        }

        _deliveryGuy = deliveryGuy;
        _state = DockState.Delivering;
        deliveryGuy.SetTargetDock(this);
        return true;
    }

    public bool TryCompleteDelivery(DeliveryGuyController deliveryGuy)
    {
        if (deliveryGuy == null || !IsDelivering || _waitingCustomer == null || _deliveryGuy != deliveryGuy)
        {
            return false;
        }

        if (!deliveryGuy.TryDeliverToCustomer(_waitingCustomer))
        {
            return false;
        }

        deliveryGuy.SetTargetDock(null);
        _deliveryGuy = null;
        _state = DockState.WaitingToBuy;
        return true;
    }

    public bool TryCompleteCustomerPurchase(CustomerController customer)
    {
        if (customer == null || !IsWaitingToBuy || _waitingCustomer != customer)
        {
            return false;
        }

        if (!customer.TryPurchaseFromOwnPackage())
        {
            return false;
        }

        customer.SetTargetDock(null);
        _waitingCustomer = null;
        _state = DockState.Empty;
        EventBus.Publish(new DockBecameEmpty(this));
        _purchaseParticleSystem?.Play();
        return true;
    }

    public void Clear(bool publishEmptyEvent = false)
    {
        _waitingCustomer = null;
        _deliveryGuy = null;
        var wasNotEmpty = _state != DockState.Empty;
        _state = DockState.Empty;

        if (publishEmptyEvent && wasNotEmpty)
        {
            EventBus.Publish(new DockBecameEmpty(this));
        }
    }

    private void OnDisable()
    {
        Clear(false);
    }
}
