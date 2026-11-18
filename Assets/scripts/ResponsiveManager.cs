using UnityEngine;
using UnityEngine.Events;

public class ResponsiveManager : MonoBehaviour
{
    #region private Fields

    public static ResponsiveManager Instance { get; private set; }
    private Vector2 _lastScreenSize;

    #endregion

    #region public Properties

    public ScreenOrientation CurrentOrientation => GetScreenOrientation();

    public DeviceType CurrentDeviceType
    {
        get => GetDeviceTypeByResolution(Screen.width, Screen.height);
    }

    public bool IsPortrait() => Screen.width < Screen.height;
    public bool IsLandscape() => Screen.width >= Screen.height;
    public Vector2 CurrentScreenSize => new Vector2(Screen.width, Screen.height);
    public UnityEvent OnScreenSizeChanged { get; private set; } = new UnityEvent();

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        if (Instance != this)
            return;
        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        Application.onBeforeRender += CheckScreenSizeChange;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= CheckScreenSizeChange;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion

    #region Private Methods

    private void CheckScreenSizeChange()
    {
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);

        if (_lastScreenSize != currentScreenSize)
        {
            _lastScreenSize = currentScreenSize;

            OnScreenSizeChanged?.Invoke();
        }
    }

    private ScreenOrientation GetScreenOrientation()
    {
        return IsPortrait()
            ? ScreenOrientation.Portrait
            : ScreenOrientation.Landscape;
    }

    private DeviceType GetDeviceTypeByResolution(int width, int height)
    {
        int shortSide = Mathf.Min(width, height);
        return shortSide >= 600 && (float)Mathf.Max(width, height) / shortSide <= 1.65f
            ? DeviceType.Tablet
            : DeviceType.Mobile;
    }

    #endregion
}

public enum ScreenOrientation
{
    Portrait,
    Landscape
}

public enum DeviceType
{
    Mobile,
    Tablet,
}
