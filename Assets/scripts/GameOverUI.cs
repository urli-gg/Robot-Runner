
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class GameOverUI : UIWindow
{
    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestText;

    public void SetScores(int score, int best, bool newBest = false)
    {
        scoreText.text = $"SCORE   {score:D5}";
        bestText.text = (newBest ? "NEW BEST   " : "BEST   ") + best.ToString("D5");
    }

    public void PlayAgain()
    {
        GameManager.Instance.RestartGame();
    }

    public void MainMenu()
    {
        GameManager.Instance.ReturnToMainMenu();
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


