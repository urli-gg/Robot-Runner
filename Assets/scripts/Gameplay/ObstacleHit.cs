using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<RunnerController>() != null)
        {
            Debug.Log("¡El robot chocó con un obstáculo!");
        }
    }
}
