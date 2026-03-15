using System.Collections.Generic;
using UnityEngine;

public class Market : SingletonWrapper<Market>
{
    [SerializeField] private List<Dock> _docks = new();

    public IReadOnlyList<Dock> Docks => _docks;

    private void OnValidate()
    {
        if (_docks == null || _docks.Count == 0)
        {
            _docks = new List<Dock>(GetComponentsInChildren<Dock>());
        }
    }

    public bool TryGetEmptyDock(out Dock dock)
    {
        for (var i = 0; i < _docks.Count; i++)
        {
            var candidate = _docks[i];
            if (candidate != null && candidate.IsEmpty)
            {
                dock = candidate;
                return true;
            }
        }

        dock = null;
        return false;
    }

    public bool TryGetAvailableDock(out Dock dock)
    {
        for (var i = 0; i < _docks.Count; i++)
        {
            var candidate = _docks[i];
            if (candidate != null && candidate.IsAvailable)
            {
                dock = candidate;
                return true;
            }
        }

        dock = null;
        return false;
    }

    public bool TryAssignCustomerToDock(CustomerController customer, out Dock dock)
    {
        dock = null;
        if (customer == null)
        {
            return false;
        }

        for (var i = 0; i < _docks.Count; i++)
        {
            var candidate = _docks[i];
            if (candidate != null && candidate.TryAssignCustomer(customer))
            {
                dock = candidate;
                return true;
            }
        }

        return false;
    }

    public bool TryReserveDockForDelivery(DeliveryGuyController deliveryGuy, out Dock dock)
    {
        dock = null;
        if (deliveryGuy == null)
        {
            return false;
        }

        for (var i = 0; i < _docks.Count; i++)
        {
            var candidate = _docks[i];
            if (candidate != null && candidate.TryMarkDelivering(deliveryGuy))
            {
                dock = candidate;
                return true;
            }
        }

        return false;
    }
}
