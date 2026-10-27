
using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        RunnerController robot =
            other.GetComponentInParent<RunnerController>();

        if (robot == null)
            return;

        Debug.Log("¡El robot chocó con un obstáculo!");

        GameManager.Instance.EndGame();
    }
}

