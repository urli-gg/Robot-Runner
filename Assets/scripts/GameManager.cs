
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UIManager UIManager;
    public string uiToShow;

    [Header("Score Settings")]
    [SerializeField] private float pointsPerSecond = 10f;

    [Header("Lives Settings")]
    [SerializeField] private int startingLives = 2;
    [SerializeField] private float invulnerabilityTime = 1.5f;

    [Header("Gameplay HUD")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private TMP_Text livesHUDText;
    [SerializeField] private TMP_Text scoreHUDText;

    private static bool startAfterReload;

    private bool isGameOver;
    private bool isPlaying;
    private bool isPaused;

    private float scoreTimer;
    private float lastDamageTime = -999f;

    private int score;
    private int bestScore;
    private int lives;

    public bool IsGameOver => isGameOver;
    public bool IsPlaying => isPlaying && !isGameOver && !isPaused;

    public int Score => score;
    public int Lives => lives;

    private void Awake()
    {
        Instance = this;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        lives = Mathf.Max(1, startingLives);

        isPlaying = startAfterReload;
        startAfterReload = false;

        Time.timeScale = isPlaying ? 1f : 0f;

        UpdateHUDText();
        UpdateHUDVisibility();
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

        int newScore = Mathf.FloorToInt(scoreTimer);

        if (newScore != score)
        {
            score = newScore;
            UpdateHUDText();
        }
    }

    public void StartGame()
    {
        if (isPlaying || isGameOver)
            return;

        isPlaying = true;

        UIManager.HideWindow("mainmenuui");
        Time.timeScale = 1f;

        UpdateHUDVisibility();
    }

    public void PauseGame()
    {
        if (!IsPlaying)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        UpdateHUDVisibility();
        UIManager.ShowWindow("pauseui");
    }

    public void ResumeGame()
    {
        if (!isPaused || isGameOver)
            return;

        isPaused = false;

        UIManager.HideWindow("pauseui");
        Time.timeScale = 1f;

        UpdateHUDVisibility();
    }

    public void TakeDamage()
    {
        if (!IsPlaying)
            return;

        // Evita perder varias vidas por el mismo choque.
        if (Time.time - lastDamageTime < invulnerabilityTime)
            return;

        lastDamageTime = Time.time;
        lives--;

        Debug.Log("Vidas restantes: " + lives);

        UpdateHUDText();

        if (lives <= 0)
            EndGame();
    }

    public void EndGame()
    {
        if (!IsPlaying)
            return;

        isGameOver = true;
        isPlaying = false;

        UpdateHUDVisibility();

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

    private void UpdateHUDText()
    {
        if (livesHUDText != null)
        {
            livesHUDText.text =
                "LIVES  <color=#FF58C8>" +
                new string('♥', Mathf.Max(0, lives)) +
                "</color>";
        }

        if (scoreHUDText != null)
        {
            scoreHUDText.text = $"SCORE: {score:D5}";
        }
    }

    private void UpdateHUDVisibility()
    {
        bool visible = IsPlaying;

        if (pauseButton != null)
            pauseButton.SetActive(visible);

        if (livesHUDText != null)
            livesHUDText.gameObject.SetActive(visible);

        if (scoreHUDText != null)
            scoreHUDText.gameObject.SetActive(visible);
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


