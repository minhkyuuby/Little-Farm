using UnityEngine;
using LittleFarm.GameplayEventSubject;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/Gameplay/Plant Clickable")]
public class PlantClickable : MonoBehaviour, IWorldClickable
{
    [SerializeField] private Plant _plant;

    private void OnValidate()
    {
        if (_plant == null)
        {
            _plant = GetComponentInParent<Plant>();
        }
    }

    public bool TryHandleWorldClick(WorldClickContext context)
    {
        if (_plant == null)
        {
            return false;
        }

        EventBus.Publish(new PlantUpgradeRequested(_plant));
        return true;
    }
}
