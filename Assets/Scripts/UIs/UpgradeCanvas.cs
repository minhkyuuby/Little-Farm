using UnityEngine;
using System.Collections.Generic;
using LittleFarm.UpgradesEventSubject;

public class UpgradeCanvas : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private UpgradeView _upgradeViewPrefab;
    [SerializeField] private ManagerUpgradeSystem _upgradeSystem;
    [SerializeField] private ProfitScaleView _profitScaleView;
    [SerializeField] private CustomerCapacityView _customerCapacityView;
    [SerializeField] private DeliveryCapacityView _deliveryCapacityView;

    private readonly List<UpgradeView> _spawnedViews = new();

    void OnValidate()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        if (_upgradeSystem == null)
        {
            _upgradeSystem = FindFirstObjectByType<ManagerUpgradeSystem>();
        }
    }

    private void Start()
    {
        TryBindUpgradeSystem();
        RebuildViews();
    }

    private void OnEnable()
    {
        TryBindUpgradeSystem();
        EventBus.Subscribe<UpgradePaid>(OnUpgradePaid);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<UpgradePaid>(OnUpgradePaid);
    }
    
    public void Show()
    {
        RebuildViews();
        RefreshSummaryViews();
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }

    public void RebuildViews()
    {
        ClearViews();

        if (_upgradeSystem == null)
        {
            _upgradeSystem = FindFirstObjectByType<ManagerUpgradeSystem>();
        }

        if (_upgradeSystem == null || _upgradeViewPrefab == null || _scrollContent == null)
        {
            return;
        }

        var upgrades = _upgradeSystem.GetUpgradesForDisplay();
        for (var i = 0; i < upgrades.Count; i++)
        {
            var upgrade = upgrades[i];
            if (upgrade == null || _upgradeSystem.IsPurchased(upgrade))
            {
                continue;
            }

            var view = Instantiate(_upgradeViewPrefab, _scrollContent);
            view.Bind(_upgradeSystem, upgrade);
            _spawnedViews.Add(view);
        }

        RefreshSummaryViews();
    }

    private void TryBindUpgradeSystem()
    {
        var resolved = _upgradeSystem != null ? _upgradeSystem : FindFirstObjectByType<ManagerUpgradeSystem>();
        if (resolved == _upgradeSystem)
        {
            return;
        }

        _upgradeSystem = resolved;
    }

    private void OnUpgradePaid(UpgradePaid message)
    {
        RebuildViews();
    }

    private void ClearViews()
    {
        for (var i = 0; i < _spawnedViews.Count; i++)
        {
            var view = _spawnedViews[i];
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }

        _spawnedViews.Clear();
    }

    private void RefreshSummaryViews()
    {
        if (_profitScaleView != null)
        {
            _profitScaleView.Refresh();
        }

        if (_customerCapacityView != null)
        {
            _customerCapacityView.Refresh();
        }

        if (_deliveryCapacityView != null)
        {
            _deliveryCapacityView.Refresh();
        }
    }
}
