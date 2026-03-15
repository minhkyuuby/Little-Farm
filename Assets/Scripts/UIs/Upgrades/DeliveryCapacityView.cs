using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class DeliveryCapacityView : MonoBehaviour
{
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private string _prefix = "Delivery Capacity: ";
    [SerializeField] private string _suffix = "";
    [SerializeField] private DeliveryGuySpawner _deliverySpawner;

    private void OnValidate()
    {
        if (_deliverySpawner == null)
        {
            _deliverySpawner = FindFirstObjectByType<DeliveryGuySpawner>();
        }
    }

    private void OnEnable()
    {
        if (_deliverySpawner == null)
        {
            _deliverySpawner = FindFirstObjectByType<DeliveryGuySpawner>();
        }

        if (_deliverySpawner != null)
        {
            _deliverySpawner.OnMaxActiveDeliveryGuysChanged += HandleMaxActiveDeliveryGuysChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (_deliverySpawner != null)
        {
            _deliverySpawner.OnMaxActiveDeliveryGuysChanged -= HandleMaxActiveDeliveryGuysChanged;
        }
    }

    public void Refresh()
    {
        if (_valueText == null)
        {
            return;
        }

        var capacity = _deliverySpawner != null ? _deliverySpawner.MaxActiveDeliveryGuys : 1;
        _valueText.text = _prefix + capacity + _suffix;
    }

    private void HandleMaxActiveDeliveryGuysChanged(int maxActive)
    {
        Refresh();
    }
}
