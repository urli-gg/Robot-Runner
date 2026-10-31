
using NaughtyAttributes;
using UnityEngine;

public class MainMenuUI : UIWindow
{
    public void Play()
    {
        GameManager.Instance.StartGame();
    }

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

