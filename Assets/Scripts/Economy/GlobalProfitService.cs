using System;
using UnityEngine;
using LittleFarm.UpgradesEventSubject;

namespace LittleFarm.Economy
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Little Farm/Economy/Global Profit Service")]
    public class GlobalProfitService : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float _defaultProfitScale = 1f;
        [SerializeField] private ManagerUpgradeSystem _upgradeSystem;

        public static GlobalProfitService Instance { get; private set; }

        public event Action<float> OnProfitScaleChanged;

        public float CurrentProfitScale { get; private set; } = 1f;

        private void OnValidate()
        {
            if (_upgradeSystem == null)
            {
                _upgradeSystem = ManagerUpgradeSystem.Instance ?? FindFirstObjectByType<ManagerUpgradeSystem>();
            }

            _defaultProfitScale = Mathf.Max(1f, _defaultProfitScale);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            CurrentProfitScale = Mathf.Max(1f, _defaultProfitScale);
            CurrencyManager.SetGlobalProfitScale(CurrentProfitScale);
            TryBindUpgradeSystem();
            RefreshFromUpgradeSystem();
        }

        private void OnEnable()
        {
            TryBindUpgradeSystem();
            EventBus.Subscribe<UpgradePaid>(HandleUpgradePaid);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<UpgradePaid>(HandleUpgradePaid);

            if (_upgradeSystem != null)
            {
                _upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_upgradeSystem != null)
            {
                _upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public long GetScaledPackageProfit(long basePackagePriceCoin)
        {
            if (basePackagePriceCoin <= 0)
            {
                return 0;
            }

            var scaled = (double)basePackagePriceCoin * CurrentProfitScale;
            var rounded = Math.Round(scaled, MidpointRounding.AwayFromZero);
            return Math.Max(0L, (long)rounded);
        }

        private void TryBindUpgradeSystem()
        {
            var resolved = _upgradeSystem != null ? _upgradeSystem : ManagerUpgradeSystem.Instance ?? FindFirstObjectByType<ManagerUpgradeSystem>();
            if (resolved == _upgradeSystem)
            {
                return;
            }

            if (_upgradeSystem != null)
            {
                _upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
            }

            _upgradeSystem = resolved;

            if (_upgradeSystem != null)
            {
                _upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
                _upgradeSystem.OnUpgradeStateChanged += HandleUpgradeStateChanged;
            }
        }

        private void HandleUpgradeStateChanged()
        {
            RefreshFromUpgradeSystem();
        }

        private void HandleUpgradePaid(UpgradePaid message)
        {
            // Re-resolve manager in case initialization order prevented early binding.
            TryBindUpgradeSystem();
            RefreshFromUpgradeSystem();
        }

        private void RefreshFromUpgradeSystem()
        {
            var nextScale = _upgradeSystem != null
                ? Mathf.Max(1f, _upgradeSystem.GlobalProfitScale)
                : Mathf.Max(1f, _defaultProfitScale);

            if (Mathf.Approximately(CurrentProfitScale, nextScale))
            {
                return;
            }

            CurrentProfitScale = nextScale;
            CurrencyManager.SetGlobalProfitScale(CurrentProfitScale);
            OnProfitScaleChanged?.Invoke(CurrentProfitScale);
        }
    }
}
