using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LittleFarm.EconomyEventSubject;

[DisallowMultipleComponent]
public class PlantUpgradeView : MonoBehaviour
{
    [SerializeField] private Image _fruitImage;
    [SerializeField] private TMP_Text _fruitNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _produceTimeText;
    [SerializeField] private TMP_Text _currentPriceText;
    [SerializeField] private TMP_Text _upgradeCostText;
    [SerializeField] private Button _upgradeButton;

    [SerializeField] private string _levelPrefix = "Lv. ";
    [SerializeField] private string _currentPricePrefix = "Price: ";
    [SerializeField] private string _upgradeCostPrefix = "Upgrade: ";
    [SerializeField] private string _coinSuffix = " coin";
    [SerializeField] private Color _upgradeCostAffordableColor = Color.white;
    [SerializeField] private Color _upgradeCostInsufficientColor = Color.red;

    private Plant _plant;
    private RectTransform _rectTransform;

    public Plant Plant => _plant;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.AddListener(HandleUpgradeClicked);
        }
    }

    private void OnDestroy()
    {
        if (_plant != null)
        {
            _plant.OnPlantDataChanged -= HandlePlantDataChanged;
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<CurrencyValueChanged>(HandleCurrencyChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CurrencyValueChanged>(HandleCurrencyChanged);
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

        if (_currentPriceText != null)
        {
            var currentPrice = CurrencyConverter.ToCurrencyText(_plant.CurrentFruitBasePriceCoin, 1, true);
            _currentPriceText.text = _currentPricePrefix + currentPrice + _coinSuffix;
        }

        if (_upgradeCostText != null)
        {
            if (_plant.CanUpgradeToNextLevel)
            {
                var canAffordUpgrade = CurrencyManager.CanAffordCoin(_plant.NextLevelUpgradeCostCoin);
                var upgradeCost = CurrencyConverter.ToCurrencyText(_plant.NextLevelUpgradeCostCoin, 1, true);
                _upgradeCostText.text = _upgradeCostPrefix + upgradeCost + _coinSuffix;
                _upgradeCostText.color = canAffordUpgrade ? _upgradeCostAffordableColor : _upgradeCostInsufficientColor;
            }
            else
            {
                _upgradeCostText.text = _upgradeCostPrefix + "MAX";
                _upgradeCostText.color = _upgradeCostAffordableColor;
            }
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.interactable = _plant.CanUpgradeToNextLevel
                && CurrencyManager.CanAffordCoin(_plant.NextLevelUpgradeCostCoin);
        }

        RefreshProduceTime();
    }

    private void HandleUpgradeClicked()
    {
        if (_plant == null)
        {
            return;
        }

        _plant.TryUpgradeToNextLevel();
        RefreshAll();
    }

    private void HandlePlantDataChanged(Plant changedPlant)
    {
        if (changedPlant == _plant)
        {
            RefreshAll();
        }
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

    private void HandleCurrencyChanged(CurrencyValueChanged evt)
    {
        if (evt.CurrencyType == CurrencyType.Coin)
        {
            RefreshAll();
        }
    }
}
