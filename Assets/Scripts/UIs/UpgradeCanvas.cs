using UnityEngine;

public class UpgradeCanvas : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    void OnValidate()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }
    }
    
    public void Show()
    {
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }
}
