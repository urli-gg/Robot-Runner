
using NaughtyAttributes;
using UnityEngine;

public class PauseUI : UIWindow
{
    public void OpenSettings() 
    {
        UIManager.Instance.ShowWindow("settingsui");
    }
        
    public void Resume()
    {
        GameManager.Instance.ResumeGame();
    }

    public void MainMenu()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

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
}

