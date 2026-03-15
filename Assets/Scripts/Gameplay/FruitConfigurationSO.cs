using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct FruitLevelConfig
{
    [SerializeField] private long _basePriceCoin;
    [SerializeField] private long _levelUnlockPriceCoin;
    [SerializeField] private float _developmentDuration;

    public long BasePriceCoin => Math.Max(0L, _basePriceCoin);
    public long LevelUnlockPriceCoin => Math.Max(0L, _levelUnlockPriceCoin);
    public float DevelopmentDuration => Mathf.Max(0.01f, _developmentDuration);
}

[CreateAssetMenu(fileName = "FruitConfiguration", menuName = "Little Farm/Gameplay/Fruit Configuration")]
public class FruitConfigurationSO : ScriptableObject
{
    [SerializeField] private Fruit _fruitPrefab;
    [SerializeField] private string _fruitDisplayName = "Fruit";
    [SerializeField] private Sprite _fruitSprite;
    [SerializeField] private string _fruitPoolId = "fruits";
    [SerializeField] private PoolSettings _fruitPoolSettings = new();
    [SerializeField] private List<FruitLevelConfig> _levelConfigs = new();
    [SerializeField] private float _restDuration = 2f;

    public Fruit FruitPrefab => _fruitPrefab;
    public string FruitDisplayName => string.IsNullOrWhiteSpace(_fruitDisplayName) ? name : _fruitDisplayName;
    public Sprite FruitSprite => _fruitSprite;
    public string FruitPoolId => _fruitPoolId;
    public PoolSettings FruitPoolSettings => _fruitPoolSettings;
    public float RestDuration => _restDuration;
    public int MaxConfiguredLevel => Mathf.Max(1, _levelConfigs.Count);

    public FruitLevelConfig GetLevelConfig(int level)
    {
        if (_levelConfigs == null || _levelConfigs.Count == 0)
        {
            return new FruitLevelConfig();
        }

        var index = Mathf.Clamp(level - 1, 0, _levelConfigs.Count - 1);
        return _levelConfigs[index];
    }
}
