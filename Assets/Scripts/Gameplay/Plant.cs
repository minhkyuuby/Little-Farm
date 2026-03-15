using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [SerializeField] private List<Transform> _fruitPositions = new();
    [SerializeField] private FruitConfigurationSO _fruitConfig;
    [SerializeField] private bool _autoStartGrowth = true;

    private readonly List<Fruit> _activeFruits = new();
    private Coroutine _growthRoutine;
    private bool _isHarvestable;

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
    }

    public bool IsHarvestable => _isHarvestable;

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
        return true;
    }

    private IEnumerator GrowthLoop()
    {
        while (true)
        {
            SpawnFruits();
            _isHarvestable = false;

            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, GetDevelopmentDuration());

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                SetFruitGrowth(t);
                yield return null;
            }

            SetFruitGrowth(1f);
            _isHarvestable = true;

            while (_isHarvestable)
            {
                yield return null;
            }

            var restDuration = GetRestDuration();
            if (restDuration > 0f)
            {
                yield return new WaitForSeconds(restDuration);
            }
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

            fruit.transform.localScale = Vector3.zero;
            _activeFruits.Add(fruit);
        }
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
        for (var i = 0; i < _activeFruits.Count; i++)
        {
            var fruit = _activeFruits[i];
            if (fruit != null)
            {
                PoolManager.Instance.Release(fruit);
            }
        }
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

    private float GetDevelopmentDuration()
    {
        return _fruitConfig != null ? _fruitConfig.DevelopmentDuration : 5f;
    }

    private float GetRestDuration()
    {
        return _fruitConfig != null ? _fruitConfig.RestDuration : 2f;
    }
}
