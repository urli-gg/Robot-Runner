using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FactoryBackdrop : MonoBehaviour
{
    public const int PixelsPerUnit = 25;
    public const int ChunkPixels = 384;
    public const float ChunkWidth = (float)ChunkPixels / PixelsPerUnit;
    public const float SecondsPerBiome = 26f;

    [SerializeField] private FactoryArt art;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private float trackHeight = -2.5f;

    private const float BlendSeconds = 2.4f;
    private readonly Layer[] layers = new Layer[3];
    private int roomSeed;
    private int biomeIndex;
    private int previousBiome;
    private float clock;

    public int BiomeIndex => biomeIndex;
    public FactoryArt.Biome CurrentBiome => art.biomes[biomeIndex];
    public event Action<FactoryArt.Biome> BiomeChanged;

    private sealed class Layer
    {
        public float Rate;
        public float Height;
        public int Order;
        public float Offset;
        public int First = int.MinValue;
        public readonly List<Chunk> Chunks = new();
    }

    private sealed class Chunk
    {
        public SpriteRenderer Current;
        public SpriteRenderer Previous;
        public SpriteRenderer Machine;
        public SpriteRenderer Steam;
    }

    private void Awake()
    {
        roomSeed = UnityEngine.Random.Range(0, 1024);
        layers[0] = new Layer { Rate = 0.16f, Height = 0f, Order = -40 };
        layers[1] = new Layer { Rate = 0.44f, Height = 0f, Order = -34 };
        layers[2] = new Layer { Rate = 1f, Height = trackHeight, Order = -10 };
        Render(0f, 1f, true);
    }

    private void LateUpdate()
    {
        GameManager game = GameManager.Instance;
        bool running = game.IsPlaying;
        float delta = running ? Time.deltaTime : game.HasStarted ? 0f : Time.unscaledDeltaTime;
        clock += delta;
        int next = Mathf.FloorToInt(spawner.SurvivalTime / SecondsPerBiome) % art.biomes.Length;
        bool changed = biomeIndex != next;
        if (changed)
        {
            previousBiome = biomeIndex;
            biomeIndex = next;
            BiomeChanged?.Invoke(CurrentBiome);
        }

        float blend = previousBiome == biomeIndex ? 1f : Mathf.SmoothStep(
            0f, 1f, Mathf.Repeat(spawner.SurvivalTime, SecondsPerBiome) / BlendSeconds);
        Render(delta * (running ? spawner.Speed : 0.5f), blend, changed);
    }

    private void Render(float distance, float blend, bool changed)
    {
        float halfWidth = worldCamera.orthographicSize * worldCamera.aspect;
        float left = worldCamera.transform.position.x - halfWidth;
        int count = Mathf.CeilToInt(2f * halfWidth / ChunkWidth) + 2;
        for (int i = 0; i < layers.Length; i++)
        {
            Layer layer = layers[i];
            layer.Offset += distance * layer.Rate;
            int first = Mathf.FloorToInt((left + layer.Offset) / ChunkWidth);
            while (layer.Chunks.Count < count)
            {
                layer.Chunks.Add(CreateChunk(layer, i == 1));
                changed = true;
            }

            bool refresh = changed || first != layer.First;
            layer.First = first;
            for (int j = 0; j < layer.Chunks.Count; j++)
            {
                int sector = first + j;
                Chunk chunk = layer.Chunks[Wrap(sector, layer.Chunks.Count)];
                float x = Snap(sector * ChunkWidth - layer.Offset);
                chunk.Current.transform.position = new Vector3(x, layer.Height, 0f);
                int room = Wrap(sector + roomSeed, CurrentBiome.rooms.Length);
                if (refresh)
                {
                    chunk.Current.sprite = Select(CurrentBiome, i, room);
                    chunk.Previous.sprite = Select(art.biomes[previousBiome], i, room);
                }

                bool blending = blend < 1f;
                chunk.Previous.enabled = blending;
                if (blending)
                    chunk.Previous.color = new Color(1f, 1f, 1f, i == 1 ? 1f - blend : 1f);
                chunk.Current.color = new Color(1f, 1f, 1f, blend);
                if (i == 1)
                    Animate(chunk, sector, room, blend);
            }
        }
    }

    private Chunk CreateChunk(Layer layer, bool animate)
    {
        SpriteRenderer current = CreateSprite("Factory segment", transform, layer.Order + 1);
        var chunk = new Chunk
        {
            Current = current,
            Previous = CreateSprite("Previous sector", current.transform, layer.Order)
        };
        if (animate)
        {
            chunk.Machine = CreateSprite("Machinery", current.transform, layer.Order + 2);
            chunk.Steam = CreateSprite("Exhaust", current.transform, layer.Order + 3);
        }
        return chunk;
    }

    private SpriteRenderer CreateSprite(string label, Transform parent, int order)
    {
        var renderer = new GameObject(label).AddComponent<SpriteRenderer>();
        renderer.transform.SetParent(parent, false);
        renderer.sharedMaterial = spriteMaterial;
        renderer.sortingOrder = order;
        return renderer;
    }

    private void Animate(Chunk chunk, int sector, int room, float blend)
    {
        var biome = blend < 0.5f ? art.biomes[previousBiome] : CurrentBiome;
        float phase = clock + Wrap(sector, 7) * 0.37f;
        Sprite[] frames = room switch
        {
            1 => biome.conveyor,
            2 => biome.arm,
            3 => biome.fluid,
            _ => null
        };
        chunk.Machine.enabled = frames != null;
        if (frames != null)
        {
            int index = room == 2
                ? Mathf.RoundToInt(Mathf.PingPong(phase * 2.3f, frames.Length - 1))
                : Mathf.FloorToInt(phase * 7f) % frames.Length;
            chunk.Machine.sprite = frames[index];
            chunk.Machine.transform.localPosition = room switch
            {
                1 => new Vector3(7.68f, -0.4f),
                2 => new Vector3(8.64f, 1.76f),
                _ => new Vector3(9.92f, 0.32f)
            };
            float alpha = blend >= 1f ? 1f : Mathf.Abs(blend * 2f - 1f);
            chunk.Machine.color = new Color(0.72f, 0.78f, 0.88f, alpha);
        }

        float exhaustPhase = Mathf.Repeat(phase, 4.8f);
        chunk.Steam.enabled = (room == 0 || room == 3) && exhaustPhase < 0.8f;
        if (chunk.Steam.enabled)
        {
            chunk.Steam.sprite = art.steam[Mathf.Min(art.steam.Length - 1, (int)(exhaustPhase * 8f))];
            chunk.Steam.transform.localPosition = new Vector3(4.48f, 1.36f + Snap(exhaustPhase * 0.6f));
            chunk.Steam.color = new Color(0.63f, 0.7f, 0.85f, 0.4f);
        }
    }

    private static Sprite Select(FactoryArt.Biome biome, int layer, int room) => layer switch
    {
        0 => biome.wall,
        1 => biome.rooms[room],
        _ => biome.deck
    };

    private static int Wrap(int value, int count) => (value % count + count) % count;
    private static float Snap(float value) => Mathf.Round(value * PixelsPerUnit) / PixelsPerUnit;
}
