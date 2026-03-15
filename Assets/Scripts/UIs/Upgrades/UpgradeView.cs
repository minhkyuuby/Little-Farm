using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LittleFarm.EconomyEventSubject;

[DisallowMultipleComponent]
public class UpgradeView : MonoBehaviour
{
    [SerializeField] private Image _thumbnailImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private TMP_Text _purchaseButtonText;

    [SerializeField] private string _costPrefix = "Cost: ";
    [SerializeField] private string _costSuffix = " coin";
    [SerializeField] private string _purchasedText = "Purchased";
    [SerializeField] private string _buyText = "Buy";
    [SerializeField] private Color _affordableCostColor = Color.white;
    [SerializeField] private Color _insufficientCostColor = Color.red;

    private ManagerUpgradeSystem _upgradeSystem;
    private ManagerUpgradeDefinition _upgradeDefinition;

    private void Awake()
    {
        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.AddListener(HandlePurchaseClicked);
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

    private void OnDestroy()
    {
        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
        }

        if (_upgradeSystem != null)
        {
            _upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
        }
    }

    public void Bind(ManagerUpgradeSystem upgradeSystem, ManagerUpgradeDefinition upgradeDefinition)
    {
        if (_upgradeSystem != null)
        {
            _upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
        }

        _upgradeSystem = upgradeSystem;
        _upgradeDefinition = upgradeDefinition;

        if (_upgradeSystem != null)
        {
            _upgradeSystem.OnUpgradeStateChanged += HandleUpgradeStateChanged;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (_upgradeDefinition == null)
        {
            return;
        }

        if (_thumbnailImage != null)
        {
            _thumbnailImage.sprite = _upgradeDefinition.Thumbnail;
            _thumbnailImage.enabled = _upgradeDefinition.Thumbnail != null;
        }

        if (_nameText != null)
        {
            _nameText.text = _upgradeDefinition.DisplayName;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = _upgradeDefinition.ShortDescription;
        }

        var isPurchased = _upgradeSystem != null && _upgradeSystem.IsPurchased(_upgradeDefinition);
        var canAfford = CurrencyManager.CanAffordCoin(_upgradeDefinition.CurrencyCost);

        if (_costText != null)
        {
            var costValue = CurrencyConverter.ToCurrencyText(_upgradeDefinition.CurrencyCost, 1, true);
            _costText.text = _costPrefix + costValue + _costSuffix;
            _costText.color = canAfford || isPurchased ? _affordableCostColor : _insufficientCostColor;
        }

        if (_purchaseButton != null)
        {
            _purchaseButton.interactable = !isPurchased && canAfford;
        }

        if (_purchaseButtonText != null)
        {
            _purchaseButtonText.text = isPurchased ? _purchasedText : _buyText;
        }
    }

    private void HandlePurchaseClicked()
    {
        if (_upgradeSystem == null || _upgradeDefinition == null)
        {
            return;
        }

        _upgradeSystem.TryPurchase(_upgradeDefinition);
        Refresh();
    }

    private void HandleUpgradeStateChanged()
    {
        Refresh();
    }

    private void HandleCurrencyChanged(CurrencyValueChanged evt)
    {
        if (evt.CurrencyType == CurrencyType.Coin)
        {
            Refresh();
        }
    }
}
