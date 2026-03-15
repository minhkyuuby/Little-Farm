using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/Gameplay/Plant View")]
public class PlantView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Plant _plant;
    [SerializeField] private SpriteRenderer _fruitSpriteRenderer;
    [SerializeField] private Image _fruitImage;

    [Header("Level")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Text _levelLegacyText;
    [SerializeField] private string _levelPrefix = "Lv. ";

    [Header("Fruit Name")]
    [SerializeField] private TMP_Text _fruitNameText;
    [SerializeField] private Text _fruitNameLegacyText;

    [Header("Production Time")]
    [SerializeField] private TMP_Text _produceTimeText;
    [SerializeField] private Text _produceTimeLegacyText;

    [Header("Base Price")]
    [SerializeField] private TMP_Text _basePriceText;
    [SerializeField] private Text _basePriceLegacyText;
    [SerializeField] private string _basePricePrefix = "Base: ";
    [SerializeField] private string _basePriceSuffix = " coin";

    [Header("Facing")]
    [SerializeField] private bool _faceCamera = true;
    [SerializeField] private Camera _targetCamera;

    private void OnValidate()
    {
        if (_plant == null)
        {
            _plant = GetComponentInParent<Plant>();
        }

        if (_levelText == null)
        {
            _levelText = GetComponentInChildren<TMP_Text>();
        }

        if (_levelLegacyText == null)
        {
            _levelLegacyText = GetComponentInChildren<Text>();
        }
    }

    private void OnEnable()
    {
        if (_plant != null)
        {
            _plant.OnPlantDataChanged += HandlePlantDataChanged;
        }

        Refresh();
    }

    private void OnDisable()
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

    private void LateUpdate()
    {
        if (!_faceCamera)
        {
            return;
        }

        if (_targetCamera == null)
        {
            _targetCamera = Camera.main;
        }

        if (_targetCamera == null)
        {
            return;
        }

        var lookDirection = _targetCamera.transform.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
    }

    public void Refresh()
    {
        if (_plant == null)
        {
            return;
        }

        SetFruitSprite(_plant.CurrentFruitSprite);
        SetText(_fruitNameText, _fruitNameLegacyText, _plant.CurrentFruitDisplayName);
        SetText(_levelText, _levelLegacyText, _levelPrefix + _plant.Level);

        var basePrice = CurrencyConverter.ToCurrencyText(_plant.CurrentFruitBasePriceCoin, 1, true);
        SetText(_basePriceText, _basePriceLegacyText, _basePricePrefix + basePrice + _basePriceSuffix);

        RefreshProduceTime();
    }

    private void RefreshProduceTime()
    {
        if (_plant == null)
        {
            return;
        }

        var cycle = _plant.CurrentProductionDuration;
        var remaining = _plant.ProductionRemainingSeconds;

        var value = _plant.IsHarvestable
            ? "Ready"
            : $"{remaining:0.0}s / {cycle:0.0}s";

        SetText(_produceTimeText, _produceTimeLegacyText, value);
    }

    private void HandlePlantDataChanged(Plant changedPlant)
    {
        if (changedPlant != _plant)
        {
            return;
        }

        Refresh();
    }

    private void SetFruitSprite(Sprite sprite)
    {
        if (_fruitSpriteRenderer != null)
        {
            _fruitSpriteRenderer.sprite = sprite;
        }

        if (_fruitImage != null)
        {
            _fruitImage.sprite = sprite;
        }
    }

    private static void SetText(TMP_Text tmp, Text legacy, string value)
    {
        if (tmp != null)
        {
            tmp.text = value;
        }

        if (legacy != null)
        {
            legacy.text = value;
        }
    }
}
