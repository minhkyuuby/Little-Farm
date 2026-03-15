using System;
using UnityEngine;

public enum ManagerUpgradeType
{
    GlobalProfitScale = 0,
    AddCustomerCapacity = 1,
    AddDeliveryCapacity = 2,
}

[Serializable]
public class ManagerUpgradeDefinition
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField, TextArea] private string _shortDescription;
    [SerializeField, Min(0)] private long _currencyCost;
    [SerializeField] private Sprite _thumbnail;
    [SerializeField] private ManagerUpgradeType _upgradeType;
    [SerializeField, Min(0f)] private float _floatValue;
    [SerializeField] private int _intValue;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string ShortDescription => _shortDescription;
    public long CurrencyCost => Math.Max(0L, _currencyCost);
    public Sprite Thumbnail => _thumbnail;
    public ManagerUpgradeType UpgradeType => _upgradeType;
    public float FloatValue => Mathf.Max(0f, _floatValue);
    public int IntValue => _intValue;

    public void Initialize(
        string id,
        string displayName,
        string shortDescription,
        long currencyCost,
        ManagerUpgradeType upgradeType,
        float floatValue,
        int intValue)
    {
        _id = id;
        _displayName = displayName;
        _shortDescription = shortDescription;
        _currencyCost = Math.Max(0L, currencyCost);
        _upgradeType = upgradeType;
        _floatValue = Mathf.Max(0f, floatValue);
        _intValue = intValue;
    }
}
