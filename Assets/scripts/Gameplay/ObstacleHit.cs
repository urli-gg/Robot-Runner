
using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        RunnerController robot =
            other.GetComponentInParent<RunnerController>();

        if (robot == null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage();
        }
    }
}


