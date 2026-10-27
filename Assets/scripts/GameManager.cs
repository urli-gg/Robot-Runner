
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UIManager UIManager;
    public string uiToShow;

    [Header("Score Settings")]
    [SerializeField] private float pointsPerSecond = 10f;

    private bool isGameOver;
    private float scoreTimer;
    private int score;
    private int bestScore;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    private void Update()
    {
        if (isGameOver)
            return;

        scoreTimer += Time.deltaTime * pointsPerSecond;
        score = Mathf.FloorToInt(scoreTimer);
    }

    public void EndGame()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("GAME OVER");

        if (score > bestScore)
        {
            bestScore = score;

            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        Time.timeScale = 0f;

        GameOverUI gameOverUI =
            UIManager.GetWindow("gameoverui") as GameOverUI;

        if (gameOverUI != null)
        {
            gameOverUI.SetScores(score, bestScore);
            UIManager.ShowWindow("gameoverui");
        }
        else
        {
            Debug.LogError("GameOverUI no encontrada.");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    [Button]
    private void Test()
    {
        UIManager.ShowWindow(uiToShow);
    }
}


