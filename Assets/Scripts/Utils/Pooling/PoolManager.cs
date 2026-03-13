using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonWrapper<PoolManager>
{
    private readonly Dictionary<string, IObjectPool> _poolsById = new();
    private readonly Dictionary<GameObject, IObjectPool> _poolsByPrefab = new();
    private readonly Dictionary<int, IObjectPool> _poolByInstanceId = new();

    public GenericObjectPool<T> CreatePool<T>(string poolId, T prefab, PoolSettings settings = null, Transform container = null)
        where T : Component
    {
        if (string.IsNullOrWhiteSpace(poolId))
        {
            Debug.LogError("Pool id cannot be null or whitespace.");
            return null;
        }

        if (prefab == null)
        {
            Debug.LogError($"Cannot create pool '{poolId}' with null prefab.");
            return null;
        }

        if (_poolsById.TryGetValue(poolId, out var existingPool))
        {
            if (existingPool is GenericObjectPool<T> typedPool)
            {
                return typedPool;
            }

            Debug.LogError($"Pool id '{poolId}' already exists with a different type.");
            return null;
        }

        settings ??= new PoolSettings();

        var resolvedContainer = container;
        if (resolvedContainer == null)
        {
            var containerObject = new GameObject($"Pool_{poolId}");
            resolvedContainer = containerObject.transform;
            resolvedContainer.SetParent(transform, false);
        }

        var pool = new GenericObjectPool<T>(poolId, prefab, settings, resolvedContainer);
        _poolsById[poolId] = pool;
        _poolsByPrefab[prefab.gameObject] = pool;
        return pool;
    }

    public T Spawn<T>(string poolId, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        if (!_poolsById.TryGetValue(poolId, out var pool))
        {
            Debug.LogError($"Pool '{poolId}' not found.");
            return null;
        }

        if (pool is not GenericObjectPool<T> typedPool)
        {
            Debug.LogError($"Pool '{poolId}' exists but does not match requested type '{typeof(T).Name}'.");
            return null;
        }

        var instance = typedPool.GetTyped(position, rotation, parent);
        if (instance != null)
        {
            _poolByInstanceId[instance.GetInstanceID()] = pool;
        }

        return instance;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        if (prefab == null)
        {
            return null;
        }

        if (!_poolsByPrefab.TryGetValue(prefab.gameObject, out var pool))
        {
            var defaultPoolId = prefab.name;
            var settings = new PoolSettings();
            pool = CreatePool(defaultPoolId, prefab, settings);
        }

        if (pool is not GenericObjectPool<T> typedPool)
        {
            return null;
        }

        var instance = typedPool.GetTyped(position, rotation, parent);
        if (instance != null)
        {
            _poolByInstanceId[instance.GetInstanceID()] = pool;
        }

        return instance;
    }

    public bool Release<T>(string poolId, T instance) where T : Component
    {
        if (instance == null)
        {
            return false;
        }

        if (!_poolsById.TryGetValue(poolId, out var pool))
        {
            return false;
        }

        if (pool is not GenericObjectPool<T> typedPool)
        {
            return false;
        }

        var released = typedPool.ReleaseTyped(instance);
        if (released)
        {
            _poolByInstanceId.Remove(instance.GetInstanceID());
        }

        return released;
    }

    public bool Release<T>(T instance) where T : Component
    {
        if (instance == null)
        {
            return false;
        }

        var instanceId = instance.GetInstanceID();
        if (!_poolByInstanceId.TryGetValue(instanceId, out var pool))
        {
            return false;
        }

        var released = pool.Release(instance.gameObject);
        if (released)
        {
            _poolByInstanceId.Remove(instanceId);
        }

        return released;
    }

    public bool HasPool(string poolId)
    {
        return _poolsById.ContainsKey(poolId);
    }

    public bool Warmup(string poolId, int count)
    {
        if (!_poolsById.TryGetValue(poolId, out var pool))
        {
            return false;
        }

        pool.Warmup(count);
        return true;
    }
}
