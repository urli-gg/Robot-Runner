
using NaughtyAttributes;
using UnityEngine;

public class MainMenuUI : UIWindow
{
    public void Play()
    {
        GameManager.Instance.StartGame();
    }

    public void OpenSettings() => UIManager.Instance.ShowWindow("settingsui");

    public void OpenCredits() => UIManager.Instance.ShowWindow("creditsui");

    #region Test Methods

    [Button("Test Show")]
    private void TestShow()
    {
        Show();
    }

    [Button("Test Hide")]
    private void TestHide()
    {
        Hide();
    }

    #endregion
}

