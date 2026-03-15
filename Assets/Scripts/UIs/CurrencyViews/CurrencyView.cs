using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/UI/Currency View")]
public class CurrencyView : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private CurrencyType _currencyType = CurrencyType.Coin;
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private Text _legacyText;

    [Header("Formatting")]
    [SerializeField] private bool _abbreviated = true;
    [SerializeField] private int _decimals = 1;
    [SerializeField] private string _prefix = string.Empty;
    [SerializeField] private string _suffix = string.Empty;

    private void OnValidate()
    {
        if (_tmpText == null)
        {
            _tmpText = GetComponentInChildren<TMP_Text>();
        }

        if (_legacyText == null)
        {
            _legacyText = GetComponentInChildren<Text>();
        }
    }

    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    public void Refresh()
    {
        var balance = CurrencyManager.GetBalance(_currencyType);
        var formatted = CurrencyConverter.ToCurrencyText(balance, _decimals, _abbreviated);
        SetText(_prefix + formatted + _suffix);
    }

    public void SetCurrencyType(CurrencyType currencyType)
    {
        _currencyType = currencyType;
        Refresh();
    }

    private void HandleCurrencyChanged(CurrencyChangedEvent evt)
    {
        if (evt.CurrencyType != _currencyType)
        {
            return;
        }

        var formatted = CurrencyConverter.ToCurrencyText(evt.NewBalance, _decimals, _abbreviated);
        SetText(_prefix + formatted + _suffix);
    }

    private void SetText(string value)
    {
        if (_tmpText != null)
        {
            _tmpText.text = value;
        }

        if (_legacyText != null)
        {
            _legacyText.text = value;
        }
    }
}
