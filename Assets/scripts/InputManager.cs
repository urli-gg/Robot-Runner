using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnDragEnd;
    public event Action<bool> OnTapEvent;
    public event Action<Vector2> OnContinuousEvent;

    private Vector2 dragStart;
    private Vector2 dragPosition;
    private bool dragging;
    private bool touching;
    private InputAction contactAction;
    private InputAction positionAction;
    private readonly List<RaycastResult> raycasts = new(16);
    private EventSystem cachedEventSystem;
    private PointerEventData pointer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InputActionMap dragMap = GetComponent<PlayerInput>().actions.FindActionMap("Drag");
        contactAction = dragMap.FindAction("PrimaryContact");
        positionAction = dragMap.FindAction("PrimaryPosition");
        contactAction.started += StartDrag;
        contactAction.canceled += FinishDrag;
        positionAction.performed += UpdateDrag;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;
        Instance = null;
        if (contactAction != null)
        {
            contactAction.started -= StartDrag;
            contactAction.canceled -= FinishDrag;
            positionAction.performed -= UpdateDrag;
        }
    }

    private void StartDrag(InputAction.CallbackContext context)
    {
        dragging = true;
        dragStart = dragPosition = positionAction.ReadValue<Vector2>();
    }

    private void UpdateDrag(InputAction.CallbackContext context)
    {
        if (dragging)
            dragPosition = context.ReadValue<Vector2>();
    }

    private void FinishDrag(InputAction.CallbackContext context)
    {
        if (!dragging)
            return;
        dragging = false;
        OnDragEnd?.Invoke((dragPosition - dragStart).normalized);
    }

    public void OnTap(InputAction.CallbackContext context)
    {
        if (context.started && GameManager.Instance != null && GameManager.Instance.IsPlaying && !IsPointerOverUI(context))
        {
            touching = true;
            OnTapEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            ReleaseTap();
        }
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            ReleaseTap();
    }

    private void ReleaseTap()
    {
        if (!touching)
            return;
        touching = false;
        OnTapEvent?.Invoke(false);
    }

    private bool IsPointerOverUI(InputAction.CallbackContext context)
    {
        EventSystem current = EventSystem.current;
        if (current == null)
            return false;

        Vector2 position;
        if (context.control.device is Touchscreen touchscreen)
            position = touchscreen.primaryTouch.position.ReadValue();
        else if (context.control.device is Mouse mouse)
            position = mouse.position.ReadValue();
        else
            return false;

        if (cachedEventSystem != current)
        {
            cachedEventSystem = current;
            pointer = new PointerEventData(current);
        }
        pointer.Reset();
        pointer.position = position;
        current.RaycastAll(pointer, raycasts);
        bool blocked = false;
        foreach (RaycastResult result in raycasts)
        {
            if (result.module is GraphicRaycaster)
            {
                blocked = true;
                break;
            }
        }
        raycasts.Clear();
        return blocked;
    }

    public void OnContinuous(InputAction.CallbackContext context) =>
        OnContinuousEvent?.Invoke(context.ReadValue<Vector2>());
}
