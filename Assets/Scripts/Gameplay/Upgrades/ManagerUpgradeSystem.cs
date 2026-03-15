using System;
using System.Collections.Generic;
using UnityEngine;
using LittleFarm.UpgradesEventSubject;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/Gameplay/Manager Upgrade System")]
public class ManagerUpgradeSystem : MonoBehaviour
{
    [SerializeField] private List<ManagerUpgradeDefinition> _upgrades = new();

    private readonly HashSet<string> _purchasedUpgradeIds = new();

    private float _globalProfitScale = 1f;
    private int _customerCapacityBonus;
    private int _deliveryCapacityBonus;

    public static ManagerUpgradeSystem Instance { get; private set; }

    public event Action OnUpgradeStateChanged;

    public float GlobalProfitScale => Mathf.Max(1f, _globalProfitScale);
    public int CustomerCapacityBonus => Mathf.Max(0, _customerCapacityBonus);
    public int DeliveryCapacityBonus => Mathf.Max(0, _deliveryCapacityBonus);

    private void OnValidate()
    {
        EnsureDefaultUpgrades();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureDefaultUpgrades();
        RecalculateDerivedState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public IReadOnlyList<ManagerUpgradeDefinition> GetUpgradesForDisplay()
    {
        var ordered = new List<ManagerUpgradeDefinition>(_upgrades);
        ordered.Sort(CompareForDisplay);
        return ordered;
    }

    public bool IsPurchased(ManagerUpgradeDefinition upgrade)
    {
        if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.Id))
        {
            return false;
        }

        return _purchasedUpgradeIds.Contains(upgrade.Id);
    }

    public bool CanPurchase(ManagerUpgradeDefinition upgrade)
    {
        if (upgrade == null || IsPurchased(upgrade))
        {
            return false;
        }

        return CurrencyManager.CanAffordCoin(upgrade.CurrencyCost);
    }

    public bool TryPurchase(ManagerUpgradeDefinition upgrade)
    {
        if (!CanPurchase(upgrade))
        {
            return false;
        }

        if (!CurrencyManager.TrySpendCoin(upgrade.CurrencyCost, $"ManagerUpgrade:{upgrade.Id}"))
        {
            return false;
        }

        _purchasedUpgradeIds.Add(upgrade.Id);
        RecalculateDerivedState();
        EventBus.Publish(new UpgradePaid(upgrade.Id));
        OnUpgradeStateChanged?.Invoke();
        return true;
    }

    private void RecalculateDerivedState()
    {
        _globalProfitScale = 1f;
        _customerCapacityBonus = 0;
        _deliveryCapacityBonus = 0;

        for (var i = 0; i < _upgrades.Count; i++)
        {
            var upgrade = _upgrades[i];
            if (upgrade == null || !IsPurchased(upgrade))
            {
                continue;
            }

            switch (upgrade.UpgradeType)
            {
                case ManagerUpgradeType.GlobalProfitScale:
                    _globalProfitScale = Mathf.Max(_globalProfitScale, upgrade.FloatValue);
                    break;
                case ManagerUpgradeType.AddCustomerCapacity:
                    _customerCapacityBonus += Mathf.Max(0, upgrade.IntValue);
                    break;
                case ManagerUpgradeType.AddDeliveryCapacity:
                    _deliveryCapacityBonus += Mathf.Max(0, upgrade.IntValue);
                    break;
            }
        }
    }

    private int CompareForDisplay(ManagerUpgradeDefinition a, ManagerUpgradeDefinition b)
    {
        if (a == null && b == null)
        {
            return 0;
        }

        if (a == null)
        {
            return 1;
        }

        if (b == null)
        {
            return -1;
        }

        if (a.UpgradeType != b.UpgradeType)
        {
            return a.UpgradeType.CompareTo(b.UpgradeType);
        }

        if (a.UpgradeType == ManagerUpgradeType.GlobalProfitScale)
        {
            return b.FloatValue.CompareTo(a.FloatValue);
        }

        return b.IntValue.CompareTo(a.IntValue);
    }

    private void EnsureDefaultUpgrades()
    {
        if (_upgrades == null)
        {
            _upgrades = new List<ManagerUpgradeDefinition>();
        }

        if (_upgrades.Count > 0)
        {
            return;
        }

        _upgrades.Add(CreateDefaultUpgrade(
            "global_profit_x3",
            "Global Benefit x3",
            "Set global profit multiplier to x3.",
            300,
            ManagerUpgradeType.GlobalProfitScale,
            3f,
            0));

        _upgrades.Add(CreateDefaultUpgrade(
            "global_profit_x2",
            "Global Benefit x2",
            "Set global profit multiplier to x2.",
            150,
            ManagerUpgradeType.GlobalProfitScale,
            2f,
            0));

        _upgrades.Add(CreateDefaultUpgrade(
            "customer_capacity_plus2",
            "Add Customer Capacity +2",
            "Increase max active customers by 2.",
            250,
            ManagerUpgradeType.AddCustomerCapacity,
            0f,
            2));

        _upgrades.Add(CreateDefaultUpgrade(
            "customer_capacity_plus1",
            "Add Customer Capacity +1",
            "Increase max active customers by 1.",
            120,
            ManagerUpgradeType.AddCustomerCapacity,
            0f,
            1));

        _upgrades.Add(CreateDefaultUpgrade(
            "delivery_capacity_plus2",
            "Add Delivery Capacity +2",
            "Increase max active delivery guys by 2.",
            250,
            ManagerUpgradeType.AddDeliveryCapacity,
            0f,
            2));

        _upgrades.Add(CreateDefaultUpgrade(
            "delivery_capacity_plus1",
            "Add Delivery Capacity +1",
            "Increase max active delivery guys by 1.",
            120,
            ManagerUpgradeType.AddDeliveryCapacity,
            0f,
            1));
    }

    private static ManagerUpgradeDefinition CreateDefaultUpgrade(
        string id,
        string name,
        string description,
        long cost,
        ManagerUpgradeType upgradeType,
        float floatValue,
        int intValue)
    {
        var definition = new ManagerUpgradeDefinition();
        definition.Initialize(id, name, description, cost, upgradeType, floatValue, intValue);
        return definition;
    }
}
