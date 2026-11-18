using UnityEngine;

public sealed class ObstacleAnimation : MonoBehaviour
{
    [SerializeField] private FactoryArt art;
    [SerializeField] private ObstacleKind kind;
    [SerializeField] private SpriteRenderer[] visuals;

    private Sprite[] frames;
    private Sprite[] idle;
    private float age;

    private void OnEnable()
    {
        age = 0f;
        if (kind == ObstacleKind.Boxes)
        {
            Sprite crate = art.crates[Random.Range(0, art.crates.Length)];
            foreach (SpriteRenderer visual in visuals)
                visual.sprite = crate;
        }
        else if (kind == ObstacleKind.Saw)
        {
            frames = art.saw;
            visuals[0].sprite = frames[0];
        }
        else
        {
            bool alternate = Random.value < 0.42f;
            frames = alternate ? art.droneAltMove : art.droneMove;
            idle = alternate ? art.droneAltIdle : art.droneIdle;
            visuals[0].sprite = idle[0];
        }
    }

    private void Update()
    {
        if (kind == ObstacleKind.Boxes || !GameManager.Instance.IsPlaying)
            return;

        age += Time.deltaTime;
        Sprite[] clip = kind == ObstacleKind.Drone && age < 0.55f ? idle : frames;
        int index = (int)(age * (kind == ObstacleKind.Saw ? 18f : 9f)) % clip.Length;
        visuals[0].sprite = clip[index];
    }
}
