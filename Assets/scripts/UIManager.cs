using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private List<UIWindow> _uiWindows;
    public List<UIWindow> UIWindows => _uiWindows;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowWindow(string windowName) => GetWindow(windowName)?.Show();
    public void HideWindow(string windowName) => GetWindow(windowName)?.Hide();

    public UIWindow GetWindow(string windowName)
    {
        foreach (UIWindow window in _uiWindows)
            if (window != null && window.Id == windowName)
                return window;

        Debug.LogError("Window not found: " + windowName, this);
        return null;
    }
}
