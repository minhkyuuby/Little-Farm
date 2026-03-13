using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool<T> : IObjectPool where T : Component
{
    private readonly Queue<T> _inactive = new();
    private readonly HashSet<T> _inactiveSet = new();
    private readonly HashSet<T> _allInstances = new();
    private readonly Transform _container;
    private readonly T _prefab;
    private readonly PoolSettings _settings;

    public string PoolId { get; }
    public GameObject Prefab => _prefab.gameObject;
    public int CountAll => _allInstances.Count;
    public int CountInactive => _inactive.Count;

    public GenericObjectPool(string poolId, T prefab, PoolSettings settings, Transform container = null)
    {
        PoolId = poolId;
        _prefab = prefab;
        _settings = settings ?? new PoolSettings();
        _container = container;

        Warmup(_settings.InitialSize);
    }

    public T GetTyped(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (_inactive.Count == 0)
        {
            if (!_settings.AutoExpand || (_settings.MaxSize > 0 && _allInstances.Count >= _settings.MaxSize))
            {
                return null;
            }

            var created = CreateInstance(_container);
            _inactive.Enqueue(created);
        }

        var instance = _inactive.Dequeue();
        _inactiveSet.Remove(instance);
        var instanceTransform = instance.transform;

        instanceTransform.SetParent(parent, false);
        instanceTransform.SetPositionAndRotation(position, rotation);

        if (!instance.gameObject.activeSelf)
        {
            instance.gameObject.SetActive(true);
        }

        if (instance is IPoolable poolable)
        {
            poolable.OnSpawned();
        }

        return instance;
    }

    public bool ReleaseTyped(T instance)
    {
        if (instance == null || !_allInstances.Contains(instance))
        {
            return false;
        }

        if (_inactiveSet.Contains(instance))
        {
            return false;
        }

        if (instance is IPoolable poolable)
        {
            poolable.OnDespawned();
        }

        var obj = instance.gameObject;
        if (obj.activeSelf)
        {
            obj.SetActive(false);
        }

        instance.transform.SetParent(_container, false);
        _inactive.Enqueue(instance);
        _inactiveSet.Add(instance);
        return true;
    }

    public void Warmup(int count)
    {
        if (count <= 0)
        {
            return;
        }

        var target = _allInstances.Count + count;

        if (_settings.MaxSize > 0)
        {
            target = Mathf.Min(target, _settings.MaxSize);
        }

        while (_allInstances.Count < target)
        {
            var instance = CreateInstance(_container);
            _inactive.Enqueue(instance);
            _inactiveSet.Add(instance);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return GetTyped(position, rotation, parent)?.gameObject;
    }

    public bool Release(GameObject instance)
    {
        if (instance == null)
        {
            return false;
        }

        var typed = instance.GetComponent<T>();
        return ReleaseTyped(typed);
    }

    private T CreateInstance(Transform parent)
    {
        var instance = Object.Instantiate(_prefab, parent);
        instance.gameObject.SetActive(false);
        _allInstances.Add(instance);
        return instance;
    }
}
