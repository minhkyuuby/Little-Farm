using UnityEngine;

public interface IObjectPool
{
    string PoolId { get; }
    GameObject Prefab { get; }
    int CountAll { get; }
    int CountInactive { get; }

    GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null);
    bool Release(GameObject instance);
    void Warmup(int count);
}
