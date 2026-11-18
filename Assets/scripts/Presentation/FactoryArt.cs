using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Robot Runner/Factory Art")]
public sealed class FactoryArt : ScriptableObject
{
    [Serializable]
    public sealed class Biome
    {
        public string name;
        public Color accent;
        public Sprite wall;
        public Sprite[] rooms;
        public Sprite deck;
        public Sprite[] conveyor;
        public Sprite[] arm;
        public Sprite[] fluid;
    }

    public Biome[] biomes;
    public Sprite[] steam;
    public Sprite[] spark;
    public Sprite[] impact;
    public Sprite[] idle;
    public Sprite[] run;
    public Sprite[] hurt;
    public Sprite[] dead;
    public Sprite[] droneIdle;
    public Sprite[] droneMove;
    public Sprite[] droneAltIdle;
    public Sprite[] droneAltMove;
    public Sprite[] saw;
    public Sprite[] crates;
}
