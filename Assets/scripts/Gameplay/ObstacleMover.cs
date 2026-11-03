using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyPositionX = -12f;

    private void Update()
    {

        // El obstáculo se desplaza hacia la izquierda.
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Se elimina cuando sale de la pantalla.
        if (transform.position.x < destroyPositionX)
        {
            Destroy(gameObject);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0.1f, newSpeed);
    }

}

