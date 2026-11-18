using UnityEngine;

public sealed class ObstacleHit : MonoBehaviour
{
    private ObstacleMover obstacle;

    private void Awake()
    {
        obstacle = GetComponentInParent<ObstacleMover>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<RunnerController>(out _))
            obstacle.Hit();
    }
}
