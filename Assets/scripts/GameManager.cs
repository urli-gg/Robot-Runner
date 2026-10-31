

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

    [Header("Gameplay UI")]
    [SerializeField] private GameObject pauseButton;

    private static bool startAfterReload;

    private bool isGameOver;
    private bool isPlaying;
    private bool isPaused;

    private float scoreTimer;
    private int score;
    private int bestScore;

    public bool IsGameOver => isGameOver;
    public bool IsPlaying => isPlaying && !isGameOver && !isPaused;

    private void Awake()
    {
        Instance = this;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);

        isPlaying = startAfterReload;
        startAfterReload = false;

        Time.timeScale = isPlaying ? 1f : 0f;

        if (pauseButton != null)
            pauseButton.SetActive(isPlaying);
    }

    private void Start()
    {
        if (isPlaying && UIManager != null)
        {
            UIWindow menu = UIManager.GetWindow("mainmenuui");

            if (menu != null)
                menu.Hide(true);
        }
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        scoreTimer += Time.deltaTime * pointsPerSecond;
        score = Mathf.FloorToInt(scoreTimer);
    }

    public void StartGame()
    {
        if (isPlaying || isGameOver)
            return;

        isPlaying = true;

        UIManager.HideWindow("mainmenuui");

        Time.timeScale = 1f;

        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void PauseGame()
    {
        if (!IsPlaying)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseButton != null)
            pauseButton.SetActive(false);

        UIManager.ShowWindow("pauseui");
    }

    public void ResumeGame()
    {
        if (!isPaused || isGameOver)
            return;

        isPaused = false;

        UIManager.HideWindow("pauseui");

        Time.timeScale = 1f;

        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void EndGame()
    {
        if (!IsPlaying)
            return;

        isGameOver = true;
        isPlaying = false;

        if (pauseButton != null)
            pauseButton.SetActive(false);

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
    }

    public void RestartGame()
    {
        startAfterReload = true;
        ReloadScene();
    }

    public void ReturnToMainMenu()
    {
        startAfterReload = false;
        ReloadScene();
    }

    private void ReloadScene()
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

