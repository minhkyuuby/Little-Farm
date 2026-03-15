using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlantInfoView : MonoBehaviour
{
    [SerializeField] private Image _fruitImage;
    [SerializeField] private TMP_Text _fruitNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _produceTimeText;
    [SerializeField] private TMP_Text _basePriceText;
    [SerializeField] private string _levelPrefix = "Lv. ";
    [SerializeField] private string _basePricePrefix = "Base: ";
    [SerializeField] private string _basePriceSuffix = " coin";

    private Plant _plant;
    private RectTransform _rectTransform;

    public Plant Plant => _plant;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        if (_plant != null)
        {
            _plant.OnPlantDataChanged -= HandlePlantDataChanged;
        }
    }

    private void Update()
    {
        RefreshProduceTime();
    }

    public void Bind(Plant plant)
    {
        if (_plant == plant)
        {
            return;
        }

        if (_plant != null)
        {
            _plant.OnPlantDataChanged -= HandlePlantDataChanged;
        }

        _plant = plant;

        if (_plant != null)
        {
            _plant.OnPlantDataChanged += HandlePlantDataChanged;
        }

        RefreshAll();
    }

    public void SetScreenPosition(Vector2 anchoredPosition)
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = anchoredPosition;
        }
    }

    public void SetVisible(bool visible)
    {
        if (gameObject.activeSelf != visible)
        {
            gameObject.SetActive(visible);
        }
    }

    public void RefreshAll()
    {
        if (_plant == null)
        {
            return;
        }

        if (_fruitImage != null)
        {
            _fruitImage.sprite = _plant.CurrentFruitSprite;
        }

        if (_fruitNameText != null)
        {
            _fruitNameText.text = _plant.CurrentFruitDisplayName;
        }

        if (_levelText != null)
        {
            _levelText.text = _levelPrefix + _plant.Level;
        }

        if (_basePriceText != null)
        {
            var value = CurrencyConverter.ToCurrencyText(_plant.CurrentFruitBasePriceCoin, 1, true);
            _basePriceText.text = _basePricePrefix + value + _basePriceSuffix;
        }

        RefreshProduceTime();
    }

    private void RefreshProduceTime()
    {
        if (_plant == null || _produceTimeText == null)
        {
            return;
        }

        var cycle = _plant.CurrentProductionDuration;
        var remaining = _plant.ProductionRemainingSeconds;
        _produceTimeText.text = _plant.IsHarvestable ? "Ready" : $"{remaining:0.0}s / {cycle:0.0}s";
    }

    private void HandlePlantDataChanged(Plant changedPlant)
    {
        if (changedPlant == _plant)
        {
            RefreshAll();
        }
    }
}
