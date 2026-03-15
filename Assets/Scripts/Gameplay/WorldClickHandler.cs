using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[AddComponentMenu("Little Farm/Gameplay/World Click Handler")]
public class WorldClickHandler : MonoBehaviour
{
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private InputActionReference _clickAction;
    [SerializeField] private InputActionReference _pointerPositionAction;
    [SerializeField] private LayerMask _raycastLayers = ~0;
    [SerializeField, Min(0.1f)] private float _rayDistance = 500f;
    [SerializeField] private bool _ignoreWhenPointerOverUI = true;
    [SerializeField] private bool _manageActionEnabledState = true;

    private readonly List<RaycastResult> _uiRaycastResults = new();

    private void OnValidate()
    {
        if (_worldCamera == null)
        {
            _worldCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        SubscribeActions();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
    }

    private void SubscribeActions()
    {
        var clickAction = _clickAction != null ? _clickAction.action : null;
        if (clickAction == null)
        {
            return;
        }

        if (_manageActionEnabledState && !clickAction.enabled)
        {
            clickAction.Enable();
        }

        clickAction.performed -= HandleClickPerformed;
        clickAction.performed += HandleClickPerformed;
    }

    private void UnsubscribeActions()
    {
        var clickAction = _clickAction != null ? _clickAction.action : null;
        if (clickAction == null)
        {
            return;
        }

        clickAction.performed -= HandleClickPerformed;

        if (_manageActionEnabledState && clickAction.enabled)
        {
            clickAction.Disable();
        }
    }

    private void HandleClickPerformed(InputAction.CallbackContext context)
    {
        var cameraToUse = _worldCamera != null ? _worldCamera : Camera.main;
        if (cameraToUse == null)
        {
            return;
        }

        var screenPosition = ReadPointerScreenPosition();

        if (_ignoreWhenPointerOverUI && IsPointerOverUiAtScreenPosition(screenPosition))
        {
            return;
        }

        var ray = cameraToUse.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out var hit, _rayDistance, _raycastLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        var clickable = hit.collider != null ? hit.collider.GetComponentInParent<IWorldClickable>() : null;
        if (clickable == null)
        {
            return;
        }

        var clickContext = new WorldClickContext(cameraToUse, screenPosition, hit);
        clickable.TryHandleWorldClick(clickContext);
    }

    private Vector2 ReadPointerScreenPosition()
    {
        if (_pointerPositionAction != null)
        {
            var action = _pointerPositionAction.action;
            if (action != null)
            {
                if (_manageActionEnabledState && !action.enabled)
                {
                    action.Enable();
                }

                return action.ReadValue<Vector2>();
            }
        }

        if (Pointer.current != null)
        {
            return Pointer.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    private bool IsPointerOverUiAtScreenPosition(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        _uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, _uiRaycastResults);
        return _uiRaycastResults.Count > 0;
    }
}
