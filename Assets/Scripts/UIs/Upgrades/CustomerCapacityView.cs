using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CustomerCapacityView : MonoBehaviour
{
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private string _prefix = "Customer Capacity: ";
    [SerializeField] private string _suffix = "";
    [SerializeField] private CustomerSpawner _customerSpawner;

    private void OnValidate()
    {
        if (_customerSpawner == null)
        {
            _customerSpawner = FindFirstObjectByType<CustomerSpawner>();
        }
    }

    private void OnEnable()
    {
        if (_customerSpawner == null)
        {
            _customerSpawner = FindFirstObjectByType<CustomerSpawner>();
        }

        if (_customerSpawner != null)
        {
            _customerSpawner.OnMaxActiveCustomersChanged += HandleMaxActiveCustomersChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (_customerSpawner != null)
        {
            _customerSpawner.OnMaxActiveCustomersChanged -= HandleMaxActiveCustomersChanged;
        }
    }

    public void Refresh()
    {
        if (_valueText == null)
        {
            return;
        }

        var capacity = _customerSpawner != null ? _customerSpawner.MaxActiveCustomers : 1;
        _valueText.text = _prefix + capacity + _suffix;
    }

    private void HandleMaxActiveCustomersChanged(int maxActive)
    {
        Refresh();
    }
}
