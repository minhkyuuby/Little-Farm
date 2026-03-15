using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    [SerializeField] private UpgradeCanvas _upgradeCanvas;

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
    }

    private void Start()
    {
        _upgradeCanvas.Hide();
    }

    public void ShowUpgradeCanvas()
    {
        _upgradeCanvas.Show();
    }
}
