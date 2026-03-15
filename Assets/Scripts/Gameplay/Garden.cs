using UnityEngine;
using System.Collections.Generic;

public class Garden : MonoBehaviour
{
    [SerializeField] private List<Plant> _plants = new();

    public IReadOnlyList<Plant> Plants => _plants;

    private void OnValidate()
    {
        RefreshPlants();
    }

    private void Start()
    {
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
}
