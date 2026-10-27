
using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Time")]
    [SerializeField] private float minSpawnTime = 1.8f;
    [SerializeField] private float maxSpawnTime = 3f;

    private IEnumerator Start()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);

            yield return new WaitForSeconds(waitTime);

            Instantiate(
                obstaclePrefab,
                spawnPoint.position,
                Quaternion.identity
            );
        }
    }
}

