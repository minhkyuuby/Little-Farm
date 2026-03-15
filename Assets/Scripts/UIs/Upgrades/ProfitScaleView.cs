using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ProfitScaleView : MonoBehaviour
{
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private string _prefix = "Profit Scale: x";
    [SerializeField] private string _suffix = "";

    private void OnEnable()
    {
        CurrencyManager.OnGlobalProfitScaleChanged += HandleGlobalProfitScaleChanged;
        Refresh();
    }

    private void OnDisable()
    {
        CurrencyManager.OnGlobalProfitScaleChanged -= HandleGlobalProfitScaleChanged;
    }

    public void Refresh()
    {
        if (_valueText == null)
        {
            return;
        }

        var scale = CurrencyManager.GlobalProfitScale;
        _valueText.text = _prefix + scale.ToString("0.0") + _suffix;
    }

    private void HandleGlobalProfitScaleChanged(float scale)
    {
        Refresh();
    }
}
