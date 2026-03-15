using System.Collections.Generic;
using UnityEngine;

public class ProductPackage : MonoBehaviour
{
    [SerializeField] private Transform _stackStartPosition;
    [SerializeField] private float _stackStepY = 0.2f;

    private readonly List<Fruit> _storedFruits = new();

    public bool IsEmpty => _storedFruits.Count == 0;
    public bool HasFruits => _storedFruits.Count > 0;
    public int Capacity => int.MaxValue;
    public int Count => _storedFruits.Count;
    public int FreeSlots => int.MaxValue;

    private Transform StackRoot => _stackStartPosition != null ? _stackStartPosition : transform;

    public bool TryLoadFruits(IReadOnlyList<Fruit> fruits)
    {
        if (fruits == null || fruits.Count == 0 || !IsEmpty)
        {
            return false;
        }

        for (var i = 0; i < fruits.Count; i++)
        {
            var fruit = fruits[i];
            if (fruit == null)
            {
                continue;
            }

            AttachFruitToTop(fruit);
        }

        return _storedFruits.Count > 0;
    }

    public bool TryDeliverTo(CustomerController customer)
    {
        if (customer == null || IsEmpty)
        {
            return false;
        }

        return customer.TryReceiveDelivery(this);
    }

    public bool TryReceiveFrom(ProductPackage source)
    {
        if (source == null)
        {
            return false;
        }

        return source.TryTransferTo(this);
    }

    public bool TryTransferTo(ProductPackage target)
    {
        if (target == null || target == this || IsEmpty)
        {
            return false;
        }

        for (var i = 0; i < _storedFruits.Count; i++)
        {
            var fruit = _storedFruits[i];
            if (fruit == null)
            {
                continue;
            }

            target.AttachFruitToTop(fruit);
        }

        _storedFruits.Clear();
        return true;
    }

    public int ReleaseAllFruitsToPool()
    {
        var releasedCount = _storedFruits.Count;
        ReleaseStoredFruits();
        return releasedCount;
    }

    private void ReleaseStoredFruits()
    {
        var poolManager = Object.FindFirstObjectByType<PoolManager>();
        if (poolManager == null)
        {
            _storedFruits.Clear();
            return;
        }

        for (var i = 0; i < _storedFruits.Count; i++)
        {
            var fruit = _storedFruits[i];
            if (fruit != null)
            {
                poolManager.Release(fruit);
            }
        }

        _storedFruits.Clear();
    }

    private void AttachFruitToTop(Fruit fruit)
    {
        if (fruit == null)
        {
            return;
        }

        var root = StackRoot;
        var offset = Vector3.up * Mathf.Max(0f, _stackStepY) * _storedFruits.Count;

        fruit.transform.SetParent(root, false);
        fruit.transform.SetPositionAndRotation(root.position + offset, root.rotation);
        fruit.transform.localScale = Vector3.one;
        _storedFruits.Add(fruit);
    }
}
