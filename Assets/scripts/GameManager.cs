using System;
using System.Collections;
using Dino.Utility.Audio;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UIManager UIManager;
    public string uiToShow;

    [SerializeField] private float pointsPerSecond = 10f;
    [SerializeField] private int startingLives = 2;
    [SerializeField] private float invulnerabilityTime = 1.5f;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private TMP_Text livesHUDText;
    [SerializeField] private TMP_Text scoreHUDText;

    private static bool startAfterReload;
    private bool isGameOver;
    private bool isPlaying;
    private bool isPaused;
    private bool recordAnnounced;
    private bool reloading;
    private float scoreTimer;
    private float lastDamageTime = float.NegativeInfinity;
    private int score;
    private int bestScore;
    private int lives;

    public event Action<bool, float> Damaged;
    public event Action NewRecord;

    public bool IsGameOver => isGameOver;
    public bool HasStarted => isPlaying || isGameOver;
    public bool IsPlaying => isPlaying && !isGameOver && !isPaused && !reloading;
    public int Score => score;
    public int Lives => lives;

    private void Awake()
    {
        Instance = this;
#if UNITY_ANDROID || UNITY_IOS
        Application.targetFrameRate = 60;
#endif
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        lives = Mathf.Max(1, startingLives);
        isPlaying = startAfterReload;
        startAfterReload = false;
        Time.timeScale = isPlaying ? 1f : 0f;
        UpdateLives();
        UpdateScore();
        UpdateHUDVisibility();
    }

    private void Start()
    {
        if (isPlaying)
            UIManager.GetWindow("mainmenuui")?.Hide(true);
        AudioManager.Instance?.SetMusicEnergy(isPlaying ? 0.78f : 0.50f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        scoreTimer += Time.deltaTime * pointsPerSecond;
        int newScore = Mathf.FloorToInt(scoreTimer);
        if (newScore == score)
            return;

        score = newScore;
        UpdateScore();
        if (!recordAnnounced && bestScore > 0 && score > bestScore)
        {
            recordAnnounced = true;
            NewRecord?.Invoke();
            AudioManager.Instance?.PlaySound("runnerRecord");
        }
    }

    public void StartGame()
    {
        if (HasStarted)
            return;
        isPlaying = true;
        UIManager.HideWindow("mainmenuui");
        Time.timeScale = 1f;
        UpdateHUDVisibility();
        AudioManager.Instance?.SetMusicEnergy(0.78f);
    }

    public void PauseGame()
    {
        if (!IsPlaying)
            return;
        isPaused = true;
        Time.timeScale = 0f;
        UpdateHUDVisibility();
        UIManager.ShowWindow("pauseui");
        AudioManager.Instance?.SetMusicEnergy(0.27f);
    }

    public void ResumeGame()
    {
        if (!isPaused || isGameOver)
            return;
        isPaused = false;
        UIManager.HideWindow("pauseui");
        Time.timeScale = 1f;
        UpdateHUDVisibility();
        AudioManager.Instance?.SetMusicEnergy(0.78f);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && IsPlaying)
            PauseGame();
    }

    public void TakeDamage()
    {
        if (!IsPlaying || Time.time - lastDamageTime < invulnerabilityTime)
            return;

        lastDamageTime = Time.time;
        lives--;
        UpdateLives();
        Damaged?.Invoke(lives <= 0, invulnerabilityTime);
        if (lives <= 0)
            EndGame();
    }

    public void EndGame()
    {
        if (!IsPlaying)
            return;

        isGameOver = true;
        isPlaying = false;
        Time.timeScale = 0f;
        UpdateHUDVisibility();

        bool newBest = score > bestScore;
        if (newBest)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
            if (!recordAnnounced)
                AudioManager.Instance?.PlaySound("runnerRecord");
        }

        AudioManager.Instance?.SetMusicEnergy(0.30f);
        StartCoroutine(ShowResult(newBest));
    }

    private IEnumerator ShowResult(bool newBest)
    {
        yield return new WaitForSecondsRealtime(0.31f);
        if (UIManager.GetWindow("gameoverui") is GameOverUI result)
        {
            result.SetScores(score, bestScore, newBest);
            result.Show();
        }
    }

    private void UpdateLives()
    {
        if (livesHUDText != null)
            livesHUDText.text = "LIVES  <color=#FF74C9>" + new string('♥', Mathf.Max(0, lives)) + "</color>";
    }

    private void UpdateScore()
    {
        if (scoreHUDText != null)
            scoreHUDText.SetText("SCORE  {0:00000}", score);
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

    public void RestartGame() => ReloadScene(true);

    public void ReturnToMainMenu() => ReloadScene(false);

    private void ReloadScene(bool startPlaying)
    {
        if (reloading)
            return;
        reloading = true;
        startAfterReload = startPlaying;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [Button]
    private void Test() => UIManager.ShowWindow(uiToShow);
}
