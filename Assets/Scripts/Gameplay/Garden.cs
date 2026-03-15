using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class Garden : MonoBehaviour
{
    [SerializeField] private List<GardenSlot> _slots = new();
    [SerializeField] private List<Plant> _plants = new();

    public IReadOnlyList<GardenSlot> Slots => _slots;
    public IReadOnlyList<Plant> Plants => _plants;

    [ContextMenu("Verify Garden")]
    public void VerifyGardenFromContextMenu()
    {
        VerifyGarden(out _);
    }

    private void OnValidate()
    {
        if (_slots == null)
        {
            _slots = new List<GardenSlot>();
        }

        if (_slots.Count == 0)
        {
            _slots = new List<GardenSlot>(GetComponentsInChildren<GardenSlot>(true));
        }

        if (!Application.isPlaying)
        {
            RefreshPlants();
        }
    }

    private void OnEnable()
    {
        BindBoxEvents();
    }

    private void OnDisable()
    {
        UnbindBoxEvents();
    }

    private void Start()
    {
        InitializeSlots();
        RefreshPlants();
    }

    public bool TryGetHarvestablePlant(out Plant plant, bool reserve = true)
    {
        if (TryFindHarvestablePlant(out plant, reserve))
        {
            return true;
        }

        // Handle stale/incomplete serialized plant lists by refreshing once and retrying.
        RefreshPlants();
        return TryFindHarvestablePlant(out plant, reserve);
    }

    public void RefreshPlants()
    {
        var refreshed = new List<Plant>();

        if (_slots != null && _slots.Count > 0)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot == null || !slot.IsPlanted)
                {
                    continue;
                }

                var plant = slot.Plant;
                if (plant != null)
                {
                    refreshed.Add(plant);
                }
            }

            _plants = refreshed;
            return;
        }

        _plants = new List<Plant>(GetComponentsInChildren<Plant>(true));
    }

    private bool TryFindHarvestablePlant(out Plant plant, bool reserve)
    {
        for (var i = 0; i < _plants.Count; i++)
        {
            var candidate = _plants[i];
            if (candidate == null)
            {
                continue;
            }

            if (reserve)
            {
                if (candidate.TryReserveForHarvest())
                {
                    plant = candidate;
                    return true;
                }

                continue;
            }

            if (candidate.IsAvailableForHarvest)
            {
                plant = candidate;
                return true;
            }
        }

        plant = null;
        return false;
    }

    public void ReleasePlantReservation(Plant plant)
    {
        if (plant == null)
        {
            return;
        }

        plant.ReleaseHarvestReservation();
    }

    public bool TryPlantFromBox(Box box)
    {
        if (box == null || _slots == null)
        {
            return false;
        }

        for (var i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot == null || slot.Box != box)
            {
                continue;
            }

            return TryPlantSlot(i);
        }

        return false;
    }

    public bool TryPlantSlot(int slotIndex)
    {
        if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Count)
        {
            return false;
        }

        var slot = _slots[slotIndex];
        if (slot == null)
        {
            return false;
        }

        if (slot.TryPlant())
        {
            RefreshPlants();
            return true;
        }

        return false;
    }

    private void InitializeSlots()
    {
        if (_slots == null)
        {
            return;
        }

        for (var i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot == null)
            {
                continue;
            }

            slot.InitializeState();
        }
    }

    private void BindBoxEvents()
    {
        if (_slots == null)
        {
            return;
        }

        for (var i = 0; i < _slots.Count; i++)
        {
            var box = _slots[i]?.Box;
            if (box == null)
            {
                continue;
            }

            box.Opened -= HandleBoxOpened;
            box.Opened += HandleBoxOpened;
        }
    }

    private void UnbindBoxEvents()
    {
        if (_slots == null)
        {
            return;
        }

        for (var i = 0; i < _slots.Count; i++)
        {
            var box = _slots[i]?.Box;
            if (box == null)
            {
                continue;
            }

            box.Opened -= HandleBoxOpened;
        }
    }

    private void HandleBoxOpened(Box box)
    {
        TryPlantFromBox(box);
    }

    public bool VerifyGarden(out string report)
    {
        if (_slots == null)
        {
            _slots = new List<GardenSlot>();
        }

        var issues = new List<string>();
        var uniqueSlots = new HashSet<GardenSlot>();
        var uniqueBoxes = new HashSet<Box>();
        var uniquePlants = new HashSet<Plant>();

        if (_slots.Count == 0)
        {
            issues.Add("Garden has no configured slots.");
        }

        for (var i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            var slotLabel = $"Slot {i}";

            if (slot == null)
            {
                issues.Add($"{slotLabel}: slot reference is missing.");
                continue;
            }

            if (!uniqueSlots.Add(slot))
            {
                issues.Add($"{slotLabel}: duplicate GardenSlot reference found.");
            }

            var box = slot.Box;
            var plant = slot.Plant;

            if (box == null)
            {
                issues.Add($"{slotLabel}: Box reference is missing.");
            }
            else if (!uniqueBoxes.Add(box))
            {
                issues.Add($"{slotLabel}: Box is assigned to multiple slots.");
            }

            if (plant == null)
            {
                issues.Add($"{slotLabel}: Plant reference is missing.");
            }
            else if (!uniquePlants.Add(plant))
            {
                issues.Add($"{slotLabel}: Plant is assigned to multiple slots.");
            }
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Garden verification for '{name}'");
        builder.AppendLine($"Slots configured: {_slots.Count}");

        if (issues.Count == 0)
        {
            builder.AppendLine("Result: OK");
            report = builder.ToString();
            Debug.Log(report, this);
            return true;
        }

        builder.AppendLine($"Result: FAILED ({issues.Count} issue(s))");
        for (var i = 0; i < issues.Count; i++)
        {
            builder.AppendLine($"- {issues[i]}");
        }

        report = builder.ToString();
        Debug.LogWarning(report, this);
        return false;
    }
}
