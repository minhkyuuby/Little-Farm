using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/UI/Plant Info Canvas")]
public class PlantInfoCanvas : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _container;
    [SerializeField] private PlantInfoView _plantInfoViewPrefab;
    [SerializeField] private Garden _garden;
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private Vector3 _worldOffset = new(0f, 2f, 0f);

    private readonly Dictionary<Plant, PlantInfoView> _viewsByPlant = new();
    private readonly List<Plant> _plantsBuffer = new();

    public bool IsVisible => _canvas != null && _canvas.enabled;

    private RectTransform ContainerRect
    {
        get
        {
            if (_container != null)
            {
                return _container;
            }

            return _canvas != null ? _canvas.GetComponent<RectTransform>() : null;
        }
    }

    private void OnValidate()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        if (_garden == null)
        {
            _garden = FindFirstObjectByType<Garden>();
        }

        if (_worldCamera == null)
        {
            _worldCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        RebuildViews();
    }

    private void LateUpdate()
    {
        UpdateViewPositions();
    }

    public void RebuildViews()
    {
        ClearViews();

        if (_garden == null || _plantInfoViewPrefab == null || ContainerRect == null)
        {
            return;
        }

        _garden.RefreshPlants();
        _plantsBuffer.Clear();

        var plants = _garden.Plants;
        for (var i = 0; i < plants.Count; i++)
        {
            var plant = plants[i];
            if (plant == null)
            {
                continue;
            }

            _plantsBuffer.Add(plant);
        }

        for (var i = 0; i < _plantsBuffer.Count; i++)
        {
            var plant = _plantsBuffer[i];
            var view = Instantiate(_plantInfoViewPrefab, ContainerRect);
            view.Bind(plant);
            _viewsByPlant[plant] = view;
        }

        UpdateViewPositions();
    }

    public void ClearViews()
    {
        foreach (var pair in _viewsByPlant)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }

        _viewsByPlant.Clear();
    }

    public void Show()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        if (_canvas != null)
        {
            _canvas.enabled = true;
        }
    }

    public void Hide()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        if (_canvas != null)
        {
            _canvas.enabled = false;
        }
    }

    public void Toggle()
    {
        if (IsVisible)
        {
            Hide();
            return;
        }

        Show();
    }

    private void UpdateViewPositions()
    {
        if (_viewsByPlant.Count == 0 || ContainerRect == null)
        {
            return;
        }

        if (_worldCamera == null)
        {
            _worldCamera = Camera.main;
        }

        if (_worldCamera == null)
        {
            return;
        }

        var eventCamera = _canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _worldCamera;

        foreach (var pair in _viewsByPlant)
        {
            var plant = pair.Key;
            var view = pair.Value;

            if (plant == null || view == null)
            {
                continue;
            }

            var worldPosition = plant.transform.position + _worldOffset;
            var screenPoint = _worldCamera.WorldToScreenPoint(worldPosition);

            if (screenPoint.z <= 0f)
            {
                view.SetVisible(false);
                continue;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ContainerRect, screenPoint, eventCamera, out var localPoint))
            {
                view.SetVisible(false);
                continue;
            }

            view.SetVisible(true);
            view.SetScreenPosition(localPoint);
        }
    }
}
