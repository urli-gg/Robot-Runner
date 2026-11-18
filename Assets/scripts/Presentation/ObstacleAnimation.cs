using UnityEngine;

public sealed class ObstacleAnimation : MonoBehaviour
{
    [SerializeField] private FactoryArt art;
    [SerializeField] private ObstacleKind kind;
    [SerializeField] private SpriteRenderer[] visuals;

    [System.Serializable]
    private sealed class BiomeClip
    {
        public Sprite[] frames;
        public Sprite[] secondary;
    }

    [SerializeField] private BiomeClip[] biomeClips;
    [SerializeField] private BoxCollider2D pressHitbox;
    [SerializeField] private float pressTop;

    private Sprite[] frames;
    private Sprite[] secondary;
    private bool playedWarning;
    private int lastFrame;
    private Sprite[] idle;
    private float age;

    public void SetBiome(int biome)
    {
        if (biomeClips == null || biomeClips.Length == 0)
            return;
        frames = biomeClips[biome].frames;
        secondary = biomeClips[biome].secondary;
    }

    private void OnEnable()
    {
        age = 0f;
        playedWarning = false;
        lastFrame = -1;
        if (biomeClips != null && biomeClips.Length > 0)
        {
            if (frames == null)
                SetBiome(0);
            AnimateHazard();
            return;
        }
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
        if (biomeClips != null && biomeClips.Length > 0)
        {
            AnimateHazard();
            if (kind == ObstacleKind.Barrier && !playedWarning && visuals[0].isVisible)
            {
                playedWarning = true;
                Dino.Utility.Audio.AudioManager.Instance?.PlaySound("barrierWarning");
            }
            return;
        }

        Sprite[] clip = kind == ObstacleKind.Drone && age < 0.55f ? idle : frames;
        int index = (int)(age * (kind == ObstacleKind.Saw ? 18f : 9f)) % clip.Length;
        visuals[0].sprite = clip[index];
    }

    private void AnimateHazard()
    {
        if (kind == ObstacleKind.Press)
        {
            // Retract in the reverse order. The lowest pose still clears a grounded runner.
            int phase = Mathf.FloorToInt(age * 5f) % 6;
            int index = phase <= 2 ? phase : 5 - phase;
            if (lastFrame == index)
                return;
            lastFrame = index;
            Sprite sprite = frames[index];
            SpriteRenderer visual = visuals[0];
            visual.sprite = sprite;
            float fullHeight = sprite.rect.height / sprite.pixelsPerUnit * visual.transform.localScale.y;
            visual.transform.localPosition = new Vector3(0f, pressTop - fullHeight * 0.5f);
            float height = fullHeight * (index == 0 ? 14f / 32f : index == 1 ? 24f / 32f : 1f);
            pressHitbox.size = new Vector2(pressHitbox.size.x, height);
            pressHitbox.offset = new Vector2(0f, pressTop - height * 0.5f);
            return;
        }

        int frame = Mathf.FloorToInt(age * (kind == ObstacleKind.Barrier ? 13f : 11f)) % frames.Length;
        if (lastFrame == frame)
            return;
        lastFrame = frame;
        visuals[0].sprite = frames[frame];
        if (kind == ObstacleKind.Barrier)
        {
            visuals[1].sprite = frames[frame];
            visuals[2].sprite = secondary[frame == 2 ? 0 : frame];
        }
    }
}
