using System.Collections.Generic;
using LittleFarm.GameplayEventSubject;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/UI/Plant Upgrade Canvas")]
public class PlantUpgradeCanvas : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _backgroundPanel;
    [SerializeField] private RectTransform _container;
    [SerializeField] private PlantUpgradeView _plantUpgradeViewPrefab;
    [SerializeField] private Garden _garden;
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private Vector3 _worldOffset = new(0f, 2.4f, 0f);

    private readonly Dictionary<Plant, PlantUpgradeView> _viewsByPlant = new();
    private readonly HashSet<Plant> _openedPlants = new();

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

    private void OnEnable()
    {
        EventBus.Subscribe<PlantUpgradeRequested>(HandlePlantUpgradeRequested);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlantUpgradeRequested>(HandlePlantUpgradeRequested);
    }

    private void Start()
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        RebuildViews();
        HideAll();
    }

    private void LateUpdate()
    {
        UpdateViewPositions();
    }

    public void RebuildViews()
    {
        ClearViews();

        if (_garden == null || _plantUpgradeViewPrefab == null || ContainerRect == null)
        {
            return;
        }

        _garden.RefreshPlants();
        var plants = _garden.Plants;
        for (var i = 0; i < plants.Count; i++)
        {
            var plant = plants[i];
            if (plant == null)
            {
                continue;
            }

            var view = Instantiate(_plantUpgradeViewPrefab, ContainerRect);
            view.Bind(plant);
            view.SetVisible(false);
            _viewsByPlant[plant] = view;
        }
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
        _openedPlants.Clear();
    }

    public void HideAll()
    {
        _backgroundPanel.interactable = false;
        _backgroundPanel.gameObject.SetActive(false);
        _openedPlants.Clear();

        foreach (var pair in _viewsByPlant)
        {
            if (pair.Value != null)
            {
                pair.Value.SetVisible(false);
            }
        }
    }

    public void TogglePlant(Plant plant)
    {
        if (plant == null)
        {
            return;
        }

        if (_viewsByPlant.Count == 0)
        {
            RebuildViews();
        }

        var view = EnsureViewForPlant(plant);
        if (view == null)
        {
            return;
        }

        if (_openedPlants.Contains(plant))
        {
            _openedPlants.Remove(plant);
            view.SetVisible(false);
            return;
        }

        _openedPlants.Add(plant);
        view.RefreshAll();
        view.SetVisible(true);
        // _backgroundPanel.gameObject.SetActive(true);
        // _backgroundPanel.interactable = true;
    }

    private void HandlePlantUpgradeRequested(PlantUpgradeRequested message)
    {
        TogglePlant(message.Plant);
    }

    private PlantUpgradeView EnsureViewForPlant(Plant plant)
    {
        if (plant == null)
        {
            return null;
        }

        if (_viewsByPlant.TryGetValue(plant, out var existingView) && existingView != null)
        {
            return existingView;
        }

        if (_plantUpgradeViewPrefab == null || ContainerRect == null)
        {
            return null;
        }

        var newView = Instantiate(_plantUpgradeViewPrefab, ContainerRect);
        newView.Bind(plant);
        newView.SetVisible(false);
        _viewsByPlant[plant] = newView;
        return newView;
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

            if (!_openedPlants.Contains(plant))
            {
                view.SetVisible(false);
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
