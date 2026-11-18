using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

internal static class FactoryArtImporter
{
    internal const string Output = "Assets/ProfessionalEdition";
    private const string Source = "Assets/assets/robot_factory/tileset/color";
    private const int Width = FactoryBackdrop.ChunkPixels;
    private const int Height = 240;
    private const int DeckHeight = 80;

    private static readonly string[] Folders = { "Nigth", "Day", "Hot", "Toxic" };
    private static readonly string[] Names = { "Night Factory", "Day Factory", "Hot Factory", "Toxic Factory" };
    private static readonly Color32[] Shadows =
    {
        new(10, 15, 30, 255), new(18, 30, 41, 255), new(32, 15, 26, 255), new(10, 27, 27, 255)
    };
    private static readonly Color32[] Steel =
    {
        new(40, 54, 87, 255), new(55, 77, 89, 255), new(83, 49, 59, 255), new(40, 73, 60, 255)
    };
    private static readonly Color32[] Signals =
    {
        new(70, 243, 248, 255), new(119, 224, 253, 255), new(255, 130, 76, 255), new(150, 247, 100, 255)
    };

    internal static FactoryArt Build()
    {
        Directory.CreateDirectory(Output + "/Scenery");
        var biomes = Enumerable.Range(0, Folders.Length).Select(i => new FactoryArt.Biome
        {
            name = Names[i], accent = Signals[i], rooms = new Sprite[4]
        }).ToArray();

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < biomes.Length; i++)
            {
                var source = new PixelSource(PathFor(i, "individual_sprites"));
                Wall(source, i).Write(SceneryPath(i, "Wall"));
                for (int room = 0; room < biomes[i].rooms.Length; room++)
                    Room(source, i, room).Write(SceneryPath(i, $"Room{room + 1}"));
                Deck(source, i).Write(SceneryPath(i, "Deck"));
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        for (int i = 0; i < biomes.Length; i++)
        {
            var biome = biomes[i];
            biome.wall = ImportComposition(SceneryPath(i, "Wall"), new Vector2(0, 0.5f));
            for (int room = 0; room < biome.rooms.Length; room++)
                biome.rooms[room] = ImportComposition(SceneryPath(i, $"Room{room + 1}"), new Vector2(0, 0.5f));
            biome.deck = ImportComposition(SceneryPath(i, "Deck"), new Vector2(0, 1));
            IDictionary<string, Sprite> props = ImportFrames(i, "props_animated",
                NamesFor("Conveyor_Loop_", 3).Concat(NamesFor("RoboticArm_Grab_PingPong_", 3))
                    .Concat(NamesFor("Fluid_Pipe_Flow_Loop_", 3))
                    .Concat(i == 1 ? NamesFor("ElectricSaw_Loop_", 3) : Array.Empty<string>()), 25, Pivot.Center);
            biome.conveyor = Clip(props, "Conveyor_Loop_", 3);
            biome.arm = Clip(props, "RoboticArm_Grab_PingPong_", 3);
            biome.fluid = Clip(props, "Fluid_Pipe_Flow_Loop_", 3);
        }

        var enemies = ImportFrames(0, "enemies_animated",
            new[] { "Bot_Patrol_Idle", "Bot_Patrol_Run", "Bot_Patrol_Damage", "Bot_Patrol_Dead",
                    "Drone_TypeA_Idle", "Drone_TypeA_Move", "Drone_TypeB_Idle", "Drone_TypeB_Move" }
                .SelectMany(prefix => NamesFor(prefix, 3)), 100, Pivot.Gameplay);
        var effects = ImportFrames(0, "vfx_animated", NamesFor("vfx_steam", 6).Concat(NamesFor("SparkA_", 3))
            .Concat(NamesFor("Plasma_Explosion", 7)), 25, Pivot.Center);
        var crates = ImportFrames(0, "props", NamesFor("Box_", 2), 100, Pivot.Center);
        var saw = ReadSprites(PathFor(1, "sheets/props_animated.png"));

        string path = Output + "/FactoryArt.asset";
        var art = AssetDatabase.LoadAssetAtPath<FactoryArt>(path);
        if (art == null)
        {
            art = ScriptableObject.CreateInstance<FactoryArt>();
            AssetDatabase.CreateAsset(art, path);
        }
        art.biomes = biomes;
        art.steam = Clip(effects, "vfx_steam", 6);
        art.spark = Clip(effects, "SparkA_", 3);
        art.impact = Clip(effects, "Plasma_Explosion", 7);
        art.idle = Clip(enemies, "Bot_Patrol_Idle", 3);
        art.run = Clip(enemies, "Bot_Patrol_Run", 3);
        art.hurt = Clip(enemies, "Bot_Patrol_Damage", 3);
        art.dead = Clip(enemies, "Bot_Patrol_Dead", 3);
        art.droneIdle = Clip(enemies, "Drone_TypeA_Idle", 3);
        art.droneMove = Clip(enemies, "Drone_TypeA_Move", 3);
        art.droneAltIdle = Clip(enemies, "Drone_TypeB_Idle", 3);
        art.droneAltMove = Clip(enemies, "Drone_TypeB_Move", 3);
        art.saw = Clip(saw, "ElectricSaw_Loop_", 3);
        art.crates = Clip(crates, "Box_", 2);
        EditorUtility.SetDirty(art);
        return art;
    }

    private static Pixels Wall(PixelSource source, int biome)
    {
        var pixels = new Pixels(Width, Height);
        Color shadow = Shadows[biome];
        Color steel = Steel[biome];
        Color signal = Signals[biome];
        pixels.Fill(0, 0, Width, Height, shadow);
        for (int y = 0; y < Height; y += 32)
            for (int x = 0; x < Width; x += 32)
                pixels.Stamp(source["terrain/Bg_Tile_plate_single"], x, y, 1, 0.22f);

        for (int x = 0; x < Width; x += 128)
        {
            pixels.Fill(x + 5, 26, 117, 143, Color.Lerp(shadow, steel, 0.2f));
            pixels.Frame(x + 13, 42, 102, 121, 3, Color.Lerp(shadow, steel, 0.65f), shadow);
            for (int pane = x + 19; pane < x + 108; pane += 18)
                pixels.Fill(pane, 47, 13, 105, Color.Lerp(shadow, steel, 0.38f));

            for (int y = 48; y < 152; y++)
                pixels.Fill(x + 18, y, 92, 1, Color.Lerp(shadow, signal, (152f - y) / 104f * 0.13f), 0.65f);

            pixels.Fill(x + 49, 83, 30, 2, Color.Lerp(steel, signal, 0.30f));
            pixels.Fill(x + 56, 88, 16, 1, Color.Lerp(steel, signal, 0.2f));
            pixels.Stamp(source["props/Girder_Vertical"], x - 16, 64, 1, 0.5f);
            pixels.Stamp(source["props/Girder_Vertical"], x - 16, 96, 1, 0.5f);
        }

        pixels.Fill(0, 156, Width, 21, shadow, 0.93f);
        pixels.Fill(0, 156, Width, 2, Color.Lerp(shadow, steel, 0.70f));
        pixels.Fill(0, 163, Width, 1, Color.Lerp(shadow, signal, 0.16f));
        pixels.Fill(0, 191, Width, 49, shadow);
        return pixels;
    }

    private static Pixels Room(PixelSource source, int biome, int room)
    {
        var pixels = new Pixels(Width, Height);
        Color shadow = Shadows[biome];
        Color steel = Steel[biome];
        Color signal = Signals[biome];
        Color trim = Color.Lerp(steel, shadow, 0.18f);
        Color dark = Color.Lerp(shadow, Color.black, 0.23f);

        pixels.Fill(0, 0, Width, 35, dark, 0.88f);
        pixels.Fill(0, 35, Width, 11, shadow, 0.9f);
        pixels.Fill(0, 36, Width, 2, trim);
        pixels.Fill(0, 43, Width, 1, Color.Lerp(steel, signal, 0.26f));
        pixels.Fill(0, 153, Width, 29, dark, 0.66f);
        pixels.Fill(0, 153, Width, 1, trim, 0.9f);
        pixels.Fill(0, 169, Width, 2, trim, 0.82f);

        for (int x = 16; x < Width; x += 32)
        {
            pixels.Stamp(source["props/Girder_Horizontal"], x - 16, 9, 1, 0.7f);
            pixels.Fill(x - 6, 173, 12, 3, trim, 0.7f);
            pixels.Fill(x - 5, 174, 10, 1, dark);
        }
        for (int column = 0; column < 2; column++)
        {
            int x = column == 0 ? 23 : 360;
            pixels.Fill(x - 5, 45, 12, 124, dark, 0.92f);
            pixels.Fill(x - 4, 45, 2, 124, trim);
            pixels.Fill(x + 3, 51, 1, 101, Color.Lerp(steel, signal, 0.32f), 0.65f);
            pixels.Fill(x - 8, 54, 18, 6, shadow);
            pixels.Fill(x - 8, 147, 18, 5, shadow);
            pixels.Stamp(source["small_props/LedOn_A"], x - 6, 132, 1, 0.65f);
        }

        int panelX = room == 1 ? 44 : 242;
        string panel = room == 0 ? "vent" : room == 3 ? "flat" : "number";
        pixels.Frame(panelX - 4, 76, 96, 70, 2, trim, shadow, 0.88f);
        for (int index = 0; index < 4; index++)
            pixels.Stamp(source[$"terrain/Bg_Panel_{panel}_2x2_0{index + 1}"],
                panelX + (index % 2) * 32, 80 + (index / 2) * 32, 1, 0.73f);
        pixels.Stamp(source["small_props/MachineA_01"], panelX + 68, 90, 1, 0.75f);
        pixels.Stamp(source["small_props/LedOn_B"], panelX + 69, 119, 1, 0.75f);

        for (int x = 74; x < 325; x += 116)
        {
            pixels.Stamp(source["small_props/Lamp_02"], x, 44, 1, 0.93f);
            pixels.Fill(x - 9, 56, 32, 1, signal, 0.55f);
            for (int y = 57; y < 75; y++)
                pixels.Fill(x - 9 - (y - 57) / 2, y, 32 + y - 57, 1, signal, (75 - y) * 0.0045f);
        }

        switch (room)
        {
            case 0:
                pixels.Frame(97, 65, 34, 87, 2, trim, dark, 0.92f);
                pixels.Stamp(source["terrain/Bg_DoorTop_Close"], 98, 71, 1, 0.78f);
                pixels.Stamp(source["terrain/Bg_DoorBottom_Close"], 98, 103, 1, 0.78f);
                pixels.Stamp(source["props/Machine_01"], 161, 113, 1, 0.86f);
                pixels.Stamp(source["props/Machine_02"], 192, 118, 1, 0.82f);
                break;
            case 1:
                pixels.Frame(152, 87, 156, 61, 2, trim, dark, 0.85f);
                pixels.Stamp(source["props_animated/Conveyor_ExtremeLeft_Loop_01"], 144, 114, 1, 0.78f);
                pixels.Stamp(source["props_animated/Conveyor_ExtremeRight_Loop_01"], 208, 114, 1, 0.78f);
                pixels.Stamp(source["props/Box_02"], 161, 93, 1, 0.53f);
                pixels.Stamp(source["props/Box_04"], 213, 94, 1, 0.61f);
                pixels.Stamp(source["props/Box_04"], 269, 113, 1, 0.68f);
                break;
            case 2:
                pixels.Frame(85, 57, 153, 94, 2, trim, dark, 0.74f);
                pixels.Stamp(source["small_props/Chain"], 135, 58, 1, 0.8f);
                pixels.Stamp(source["small_props/Chain"], 135, 72, 1, 0.7f);
                pixels.Stamp(source["props/Valve_Machine_02"], 112, 114, 1, 0.84f);
                pixels.Stamp(source["small_props/Robot_Part_01"], 197, 117, 1, 0.8f);
                pixels.Stamp(source["small_props/Metal_Piece"], 211, 127, 1, 0.76f);
                break;
            default:
                pixels.Frame(58, 76, 172, 73, 2, trim, dark, 0.86f);
                pixels.Stamp(source["props_animated/Energy_Reactor_Destroy_01"], 130, 89, 2, 0.85f);
                pixels.Stamp(source["props/Valve_Machine_01"], 72, 117, 1, 0.8f);
                pixels.Fill(208, 135, 40, 12, Color.Lerp(shadow, signal, 0.35f));
                pixels.Fill(208, 135, 40, 1, signal, 0.63f);
                break;
        }

        pixels.Stamp(source["small_props/PipeB_04"], 35, 60, 1, 0.8f);
        for (int x = 49; x < 119; x += 14)
            pixels.Stamp(source["small_props/PipeB_12"], x, 60, 1, 0.8f);
        pixels.Stamp(source["small_props/PipeB_14"], 119, 60, 1, 0.8f);
        pixels.Stamp(source["small_props/Wire_02"], 318, 46, 1, 0.78f);
        return pixels;
    }

    private static Pixels Deck(PixelSource source, int biome)
    {
        var pixels = new Pixels(Width, DeckHeight);
        Color shadow = Shadows[biome];
        Color steel = Steel[biome];
        Color accent = Signals[biome];
        pixels.Fill(0, 0, Width, DeckHeight, shadow);
        for (int x = 0; x < Width; x += 32)
        {
            pixels.Stamp(source["terrain/Tile_Neon_Plate_02"], x, 0, 1, 0.84f);
            pixels.Stamp(source["terrain/Tile_Iron_Grid_09"], x, 29, 1, 0.54f);
            pixels.Stamp(source["props/Girder_Horizontal"], x, 47, 1, 0.63f);
        }
        pixels.Fill(0, 0, Width, 1, Color.Lerp(steel, accent, 0.8f));
        pixels.Fill(0, 3, Width, 1, Color.Lerp(steel, accent, 0.37f));
        pixels.Fill(0, 24, Width, 5, shadow);
        pixels.Fill(0, 28, Width, 1, steel);
        for (int x = 12; x < Width; x += 24)
        {
            pixels.Fill(x, 9, 10, 3, steel, 0.95f);
            pixels.Fill(x + 2, 10, 6, 1, accent, 0.55f);
        }
        return pixels;
    }

    internal static Sprite[] HazardClip(int biome, string category, string prefix, int pixelsPerUnit = 25)
    {
        var names = NamesFor(prefix, 3);
        return Clip(ImportFrames(biome, category, names, pixelsPerUnit, Pivot.Gameplay), prefix, 3);
    }

    private enum Pivot { Center, Gameplay }

    private static IDictionary<string, Sprite> ImportFrames(int biome, string category,
        IEnumerable<string> names, int pixelsPerUnit, Pivot alignment)
    {
        string root = PathFor(biome, "sheets/" + category);
        var importer = (TextureImporter)AssetImporter.GetAtPath(root + ".png");
        SetPixelSettings(importer, pixelsPerUnit);
        importer.spriteImportMode = SpriteImportMode.Multiple;
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var existing = provider.GetSpriteRects().ToList();
        var fileNames = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        var mappings = fileNames.GetNameFileIdPairs().ToList();

        XElement xml = XDocument.Load(root + ".xml").Root;
        int sheetHeight = (int)xml.Attribute("height");
        var frames = xml.Elements("sprite").ToDictionary(node => Path.GetFileNameWithoutExtension((string)node.Attribute("n")));
        foreach (string name in names)
        {
            XElement node = frames[name];
            int x = (int)node.Attribute("x");
            int y = (int)node.Attribute("y");
            int width = (int)node.Attribute("w");
            int height = (int)node.Attribute("h");
            Vector2 pivot = new(0.5f, 0.5f);
            if (alignment == Pivot.Gameplay && name.StartsWith("Bot_", StringComparison.Ordinal))
                pivot.y = 0.44f;

            SpriteRect rect = existing.Find(entry => entry.name == name);
            if (rect == null)
            {
                rect = new SpriteRect { name = name, spriteID = GUID.Generate() };
                existing.Add(rect);
                mappings.Add(new SpriteNameFileIdPair(rect.name, rect.spriteID));
            }
            rect.rect = new Rect(x, sheetHeight - y - height, width, height);
            rect.alignment = SpriteAlignment.Custom;
            rect.pivot = pivot;
        }
        provider.SetSpriteRects(existing.ToArray());
        fileNames.SetNameFileIdPairs(mappings);
        provider.Apply();
        importer.SaveAndReimport();
        return ReadSprites(root + ".png");
    }

    private static IDictionary<string, Sprite> ReadSprites(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);

    private static IEnumerable<string> NamesFor(string prefix, int count) =>
        Enumerable.Range(1, count).Select(index => $"{prefix}{index:00}");

    private static Sprite[] Clip(IDictionary<string, Sprite> frames, string prefix, int count) =>
        NamesFor(prefix, count).Select(name => frames[name]).ToArray();

    internal static Sprite ImportComposition(string path, Vector2 pivot, int pixelsPerUnit = FactoryBackdrop.PixelsPerUnit,
        Vector4 border = default)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        SetPixelSettings(importer, pixelsPerUnit);
        importer.spriteImportMode = SpriteImportMode.Single;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        settings.spriteBorder = border;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SetPixelSettings(TextureImporter importer, int pixelsPerUnit)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }

    private static string PathFor(int biome, string relative) => $"{Source}/Biome_{Folders[biome]}_Factory/{relative}";
    private static string SceneryPath(int biome, string name) => $"{Output}/Scenery/{Folders[biome]}_{name}.png";

    private sealed class PixelSource
    {
        private readonly string root;
        private readonly Dictionary<string, Pixels> cache = new();

        public PixelSource(string root) => this.root = root;

        public Pixels this[string key]
        {
            get
            {
                if (cache.TryGetValue(key, out Pixels image))
                    return image;

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                string path = $"{root}/{key}.png";
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                    throw new IOException($"Cannot read {path}");
                image = new Pixels(texture.width, texture.height, texture.GetPixels32());
                UnityEngine.Object.DestroyImmediate(texture);
                cache.Add(key, image);
                return image;
            }
        }
    }

    internal sealed class Pixels
    {
        private readonly int width;
        private readonly int height;
        private readonly Color32[] data;

        public Pixels(int width, int height, Color32[] pixels = null)
        {
            this.width = width;
            this.height = height;
            data = pixels ?? new Color32[width * height];
        }

        public void Fill(int x, int y, int w, int h, Color color, float opacity = 1f)
        {
            color.a *= opacity;
            for (int row = Mathf.Max(0, y); row < Mathf.Min(height, y + h); row++)
                for (int col = Mathf.Max(0, x); col < Mathf.Min(width, x + w); col++)
                    Put(col, row, color);
        }

        public void Frame(int x, int y, int w, int h, int edge, Color outline, Color fill, float opacity = 1f)
        {
            Fill(x, y, w, h, outline, opacity);
            Fill(x + edge, y + edge, w - edge * 2, h - edge * 2, fill);
        }

        public void Stamp(Pixels source, int x, int y, int scale, float opacity)
        {
            for (int row = 0; row < source.height; row++)
            {
                for (int col = 0; col < source.width; col++)
                {
                    Color color = source.data[(source.height - row - 1) * source.width + col];
                    if (color.a > 0)
                        Fill(x + col * scale, y + row * scale, scale, scale, color, opacity);
                }
            }
        }

        private void Put(int x, int y, Color foreground)
        {
            int index = (height - y - 1) * width + x;
            Color background = data[index];
            if (foreground.a >= 1f || background.a <= 0f)
            {
                data[index] = foreground;
                return;
            }
            float remaining = (1f - foreground.a) * background.a;
            float alpha = foreground.a + remaining;
            Color blended = (foreground * foreground.a + background * remaining) / alpha;
            blended.a = alpha;
            data[index] = blended;
        }

        public void Write(string path)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(data);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
