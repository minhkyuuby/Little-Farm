using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    [SerializeField] private UpgradeCanvas _upgradeCanvas;
    [SerializeField] private PlantInfoCanvas _plantInfoCanvas;

    void OnValidate()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        if (_upgradeCanvas == null)
        {
            _upgradeCanvas = FindFirstObjectByType<UpgradeCanvas>();
        }

        if (_plantInfoCanvas == null)
        {
            _plantInfoCanvas = FindFirstObjectByType<PlantInfoCanvas>();
        }
    }

    private void Start()
    {
        if (_upgradeCanvas != null)
        {
            _upgradeCanvas.Hide();
        }
    }

    public void ShowUpgradeCanvas()
    {
        if (_upgradeCanvas != null)
        {
            _upgradeCanvas.Show();
        }
    }

    public void TogglePlantInfoCanvas()
    {
        if (_plantInfoCanvas == null)
        {
            _plantInfoCanvas = FindFirstObjectByType<PlantInfoCanvas>();
        }

        if (_plantInfoCanvas != null)
        {
            _plantInfoCanvas.Toggle();
        }
    }
}
