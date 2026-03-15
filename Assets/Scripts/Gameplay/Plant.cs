using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LittleFarm.GameplayEventSubject;
using System;

public class Plant : MonoBehaviour
{
    [SerializeField] private List<Transform> _fruitPositions = new();
    [SerializeField] private FruitConfigurationSO _fruitConfig;
    [SerializeField, Min(1)] private int _level = 1;
    [SerializeField] private bool _autoStartGrowth = true;

    private readonly List<Fruit> _activeFruits = new();
    private Coroutine _growthRoutine;
    private bool _isHarvestable;
    private bool _isReservedForHarvest;
    private float _productionRemainingSeconds;

    public event Action<Plant> OnPlantDataChanged;

    private void Start()
    {
        EnsureFruitPool();

        if (_autoStartGrowth)
        {
            StartGrowthLoop();
        }
    }

    private void OnDisable()
    {
        if (_growthRoutine != null)
        {
            StopCoroutine(_growthRoutine);
            _growthRoutine = null;
        }

        ReleaseFruitsToPool();
        _isHarvestable = false;
        _isReservedForHarvest = false;
    }

    public bool IsHarvestable => _isHarvestable;
    public bool IsReservedForHarvest => _isReservedForHarvest;
    public bool IsAvailableForHarvest => _isHarvestable && !_isReservedForHarvest;
    public int Level => _level;
    public long CurrentFruitBasePriceCoin => GetCurrentLevelConfig().BasePriceCoin;
    public float CurrentDevelopmentDuration => GetCurrentLevelConfig().DevelopmentDuration;
    public float CurrentRestDuration => GetRestDuration();
    public float CurrentProductionDuration => CurrentDevelopmentDuration + CurrentRestDuration;
    public float ProductionRemainingSeconds => Mathf.Max(0f, _productionRemainingSeconds);
    public string CurrentFruitDisplayName => _fruitConfig != null ? _fruitConfig.FruitDisplayName : "Fruit";
    public Sprite CurrentFruitSprite => _fruitConfig != null ? _fruitConfig.FruitSprite : null;

    public void SetLevel(int level)
    {
        _level = Mathf.Max(1, level);
        NotifyPlantDataChanged();
    }

    public void UpgradeLevel(int amount = 1)
    {
        SetLevel(_level + Mathf.Max(0, amount));
    }

    public bool TryReserveForHarvest()
    {
        if (!IsAvailableForHarvest)
        {
            return false;
        }

        _isReservedForHarvest = true;
        return true;
    }

    public void ReleaseHarvestReservation()
    {
        _isReservedForHarvest = false;
    }

    public void StartGrowthLoop()
    {
        if (_growthRoutine != null)
        {
            StopCoroutine(_growthRoutine);
        }

        _growthRoutine = StartCoroutine(GrowthLoop());
    }

    public bool TryHarvest(ProductPackage package)
    {
        if (!_isHarvestable || package == null || _activeFruits.Count == 0)
        {
            return false;
        }

        if (!package.TryLoadFruits(_activeFruits))
        {
            return false;
        }

        _activeFruits.Clear();
        _isHarvestable = false;
        _isReservedForHarvest = false;
        return true;
    }

    private IEnumerator GrowthLoop()
    {
        while (true)
        {
            SpawnFruits();
            _isHarvestable = false;
            _isReservedForHarvest = false;

            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, CurrentDevelopmentDuration);
            var restDuration = CurrentRestDuration;
            _productionRemainingSeconds = duration + restDuration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                SetFruitGrowth(t);
                _productionRemainingSeconds = Mathf.Max(0f, (duration - elapsed) + restDuration);
                yield return null;
            }

            SetFruitGrowth(1f);
            _isHarvestable = true;
            _productionRemainingSeconds = 0f;
            EventBus.Publish(new PlantProduced(this));
            NotifyPlantDataChanged();

            while (_isHarvestable)
            {
                yield return null;
            }

            if (restDuration > 0f)
            {
                var restElapsed = 0f;
                while (restElapsed < restDuration)
                {
                    restElapsed += Time.deltaTime;
                    _productionRemainingSeconds = Mathf.Max(0f, restDuration - restElapsed);
                    yield return null;
                }
            }

            _productionRemainingSeconds = 0f;
        }
    }

    private void EnsureFruitPool()
    {
        var fruitPrefab = GetFruitPrefab();
        if (fruitPrefab == null)
        {
            Debug.LogWarning("Plant missing fruit configuration or fruit prefab.");
            return;
        }

        var poolId = GetPoolId();
        var poolSettings = GetPoolSettings();

        if (!PoolManager.Instance.HasPool(poolId))
        {
            PoolManager.Instance.CreatePool(poolId, fruitPrefab, poolSettings);
        }
    }

    private void SpawnFruits()
    {
        ReleaseFruitsToPool();
        _activeFruits.Clear();

        var fruitPrefab = GetFruitPrefab();
        var poolId = GetPoolId();
        var levelConfig = GetCurrentLevelConfig();

        if (_fruitPositions.Count == 0 || fruitPrefab == null)
        {
            return;
        }

        for (var i = 0; i < _fruitPositions.Count; i++)
        {
            var position = _fruitPositions[i];
            if (position == null)
            {
                continue;
            }

            var fruit = PoolManager.Instance.Spawn<Fruit>(poolId, position.position, position.rotation, position);
            if (fruit == null)
            {
                continue;
            }

            fruit.Configure(_level, CurrentFruitSprite, levelConfig.BasePriceCoin);
            fruit.transform.localScale = Vector3.zero;
            _activeFruits.Add(fruit);
        }

        NotifyPlantDataChanged();
    }

    private void SetFruitGrowth(float t)
    {
        var scale = Vector3.one * Mathf.Clamp01(t);
        for (var i = 0; i < _activeFruits.Count; i++)
        {
            var fruit = _activeFruits[i];
            if (fruit == null)
            {
                continue;
            }

            fruit.transform.localScale = scale;
        }
    }

    private void ReleaseFruitsToPool()
    {
        var poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManager>();
        if (poolManager == null)
        {
            _activeFruits.Clear();
            return;
        }

        for (var i = 0; i < _activeFruits.Count; i++)
        {
            var fruit = _activeFruits[i];
            if (fruit != null)
            {
                poolManager.Release(fruit);
            }
        }

        _activeFruits.Clear();
    }

    private Fruit GetFruitPrefab()
    {
        return _fruitConfig != null ? _fruitConfig.FruitPrefab : null;
    }

    private string GetPoolId()
    {
        if (_fruitConfig == null || string.IsNullOrWhiteSpace(_fruitConfig.FruitPoolId))
        {
            return "fruits";
        }

        return _fruitConfig.FruitPoolId;
    }

    private PoolSettings GetPoolSettings()
    {
        if (_fruitConfig == null || _fruitConfig.FruitPoolSettings == null)
        {
            return new PoolSettings();
        }

        return _fruitConfig.FruitPoolSettings;
    }

    private float GetRestDuration()
    {
        return _fruitConfig != null ? _fruitConfig.RestDuration : 2f;
    }

    private FruitLevelConfig GetCurrentLevelConfig()
    {
        if (_fruitConfig == null)
        {
            return new FruitLevelConfig();
        }

        return _fruitConfig.GetLevelConfig(_level);
    }

    private void NotifyPlantDataChanged()
    {
        OnPlantDataChanged?.Invoke(this);
    }
}
