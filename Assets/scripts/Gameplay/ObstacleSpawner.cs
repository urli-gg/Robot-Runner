
using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Time")]
    [SerializeField] private float minSpawnTime = 3.2f;
    [SerializeField] private float maxSpawnTime = 4.2f;

    [Header("Progressive Difficulty")]
    [SerializeField] private float secondsPerLevel = 25f;
    [SerializeField] private float timeReductionPerLevel = 0.65f;
    [SerializeField] private float minimumSpawnTime = 1.45f;
    [SerializeField] private int maxLevel = 4;

    [Header("Obstacle Speed")]
    [SerializeField] private float baseObstacleSpeed = 5f;
    [SerializeField] private float speedIncreasePerLevel = 0.8f;

    private float survivalTime;
    private int currentLevel = 1;

    private void Update()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
            return;

        survivalTime += Time.deltaTime;

        int newLevel = Mathf.Clamp(
            Mathf.FloorToInt(
                survivalTime / Mathf.Max(1f, secondsPerLevel)
            ) + 1,
            1,
            Mathf.Max(1, maxLevel)
        );

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;

            Debug.Log(
                "NIVEL DE DIFICULTAD: " + currentLevel
            );
        }
    }

    private IEnumerator Start()
    {
        if (spawnPoint == null ||
            obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            Debug.LogError(
                "Faltan obstáculos o SpawnPoint."
            );

            yield break;
        }

        while (true)
        {
            if (GameManager.Instance == null ||
                !GameManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            float reduction =
                (currentLevel - 1) * timeReductionPerLevel;

            float currentMin = Mathf.Max(
                minimumSpawnTime,
                minSpawnTime - reduction
            );

            float currentMax = Mathf.Max(
                currentMin + 0.8f,
                maxSpawnTime - reduction
            );

            float waitTime = Random.Range(
                currentMin,
                currentMax
            );

            yield return new WaitForSeconds(waitTime);

            if (GameManager.Instance == null ||
                !GameManager.Instance.IsPlaying)
                continue;

            GameObject selectedPrefab =
                obstaclePrefabs[
                    Random.Range(0, obstaclePrefabs.Length)
                ];

            if (selectedPrefab == null)
                continue;

            GameObject newObstacle = Instantiate(
                selectedPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            ObstacleMover mover =
                newObstacle.GetComponent<ObstacleMover>();

            if (mover != null)
            {
                float newSpeed =
                    baseObstacleSpeed +
                    (currentLevel - 1) *
                    speedIncreasePerLevel;

                mover.SetSpeed(newSpeed);
            }
        }
    }
}




