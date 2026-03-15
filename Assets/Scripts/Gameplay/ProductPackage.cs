using System.Collections.Generic;
using UnityEngine;

public class ProductPackage : MonoBehaviour
{
    [SerializeField] private List<Transform> _fruitPositions = new();

    private readonly List<Fruit> _storedFruits = new();

    public bool IsEmpty => _storedFruits.Count == 0;
    public bool HasFruits => _storedFruits.Count > 0;
    public int Capacity => _fruitPositions.Count;
    public int Count => _storedFruits.Count;
    public int FreeSlots => Mathf.Max(0, Capacity - Count);

    public bool TryLoadFruits(IReadOnlyList<Fruit> fruits)
    {
        if (fruits == null || fruits.Count == 0 || !IsEmpty)
        {
            return false;
        }

        var count = Mathf.Min(fruits.Count, _fruitPositions.Count);
        if (count == 0)
        {
            return false;
        }

        for (var i = 0; i < count; i++)
        {
            var fruit = fruits[i];
            var slot = _fruitPositions[i];
            if (fruit == null || slot == null)
            {
                continue;
            }

            fruit.transform.SetParent(slot, false);
            fruit.transform.SetPositionAndRotation(slot.position, slot.rotation);
            fruit.transform.localScale = Vector3.one;
            _storedFruits.Add(fruit);
        }

        for (var i = count; i < fruits.Count; i++)
        {
            var extraFruit = fruits[i];
            if (extraFruit != null)
            {
                PoolManager.Instance.Release(extraFruit);
            }
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

        if (target.FreeSlots < Count)
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

            target.AttachFruitToNextFreeSlot(fruit);
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
        for (var i = 0; i < _storedFruits.Count; i++)
        {
            var fruit = _storedFruits[i];
            if (fruit != null)
            {
                PoolManager.Instance.Release(fruit);
            }
        }

        _storedFruits.Clear();
    }

    private void AttachFruitToNextFreeSlot(Fruit fruit)
    {
        if (fruit == null)
        {
            return;
        }

        for (var i = 0; i < _fruitPositions.Count; i++)
        {
            var slot = _fruitPositions[i];
            if (slot == null || IsSlotOccupied(slot))
            {
                continue;
            }

            fruit.transform.SetParent(slot, false);
            fruit.transform.SetPositionAndRotation(slot.position, slot.rotation);
            fruit.transform.localScale = Vector3.one;
            _storedFruits.Add(fruit);
            return;
        }
    }

    private bool IsSlotOccupied(Transform slot)
    {
        for (var i = 0; i < _storedFruits.Count; i++)
        {
            var fruit = _storedFruits[i];
            if (fruit != null && fruit.transform.parent == slot)
            {
                return true;
            }
        }

        return false;
    }
}
