using UnityEngine;

public class GardenSlot : MonoBehaviour
{
    [SerializeField] private Box _box;
    [SerializeField] private Plant _plant;
    [SerializeField] private ParticleSystem _plantEffect;
    [SerializeField] private bool _startPlanted;

    private bool _isPlanted;

    public Box Box => _box;
    public Plant Plant => _plant;
    public bool StartPlanted => _startPlanted;
    public bool IsPlanted => _isPlanted;

    private void OnValidate()
    {
        if (_box == null)
        {
            _box = GetComponentInChildren<Box>(true);
        }

        if (_plant == null)
        {
            _plant = GetComponentInChildren<Plant>(true);
        }
    }

    public void InitializeState()
    {
        var planted = _startPlanted || _box == null;
        ApplyPlantedState(planted);
    }

    public bool TryPlant()
    {
        if (_isPlanted)
        {
            return false;
        }

        ApplyPlantedState(true);
        return true;
    }

    private void ApplyPlantedState(bool planted)
    {
        _isPlanted = planted;

        if (_plant != null)
        {
            _plant.gameObject.SetActive(planted);
        }
    
        if (_plantEffect != null)
        {
            if (planted)
            {
                _plantEffect.Play();
            }
            else
            {
                _plantEffect.Stop();
            }
        }

        if (_box != null)
        {
            _box.ResetBoxState(!planted);
        }
    }
}
