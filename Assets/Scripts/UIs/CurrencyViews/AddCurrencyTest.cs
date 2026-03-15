using UnityEngine;
using UnityEngine.UI;

public class AddCurrencyTest : MonoBehaviour
{
     [Header("Binding")]
    [SerializeField] private CurrencyType _currencyType = CurrencyType.Coin;

    [SerializeField] private Button _currencyButton;

    public int AmountToAdd = 10000;

    void OnValidate()
    {
        if (_currencyButton == null)
        {
            _currencyButton = GetComponent<Button>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currencyButton.onClick.AddListener(AddCurrency);
    }

    private void AddCurrency()
    {
        CurrencyManager.Add(_currencyType, AmountToAdd, "AddCurrencyTest");
    }
}
