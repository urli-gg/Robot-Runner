using System;
using System.IO;
using System.Text;
using Dino.Utility.Audio;
using UnityEditor;
using UnityEngine;
using AudioType = Dino.Utility.Audio.AudioType;

internal static class RunnerAudioImporter
{
    private const int Rate = 24000;
    private const string Root = FactoryArtImporter.Output + "/Audio";

    internal static void Build()
    {
        Directory.CreateDirectory(Root);
        var data = AssetDatabase.LoadAssetAtPath<AudioManagerData>("Assets/ScriptableObjects/AudioManagerData.asset");
        var click = data.GetAudioData("click");
        click.volume = 0.60f;
        Add(data, "runnerJump", 0.13, 0.67f, (t, noise) => Sweep(350, 2400, t) * 0.75);
        Add(data, "runnerLand", 0.11, 0.36f, (t, noise) => Sweep(175, -830, t) * 0.55 + noise * 0.24);
        Add(data, "runnerHit", 0.23, 0.84f, (t, noise) => Sweep(370, -920, t) * 0.50 + noise * 0.32);
        Add(data, "runnerShutdown", 0.39, 0.77f, (t, noise) =>
            Sweep(244, -380, t) * 0.56 + Sweep(124, -210, t) * 0.26 + noise * 0.13);
        Add(data, "runnerRecord", 0.54, 0.68f, (t, noise) =>
        {
            int note = (int)(t / 0.13);
            double local = t - note * 0.13;
            double frequency = note switch { 0 => 523.25, 1 => 659.25, 2 => 783.99, _ => 1046.50 };
            return (Sweep(frequency, 0, local) * 0.62 + Sweep(frequency * 2, 0, local) * 0.13) *
                   Math.Min(1, local * 170) * Math.Max(0, 1 - local * 7.5);
        });
        Add(data, "sectorEntry", 0.28, 0.45f, (t, noise) =>
            Sweep(420, 390, t) * 0.46 + Sweep(628, 590, t) * 0.24);
        EditorUtility.SetDirty(data);
    }

    private static double Sweep(double frequency, double slope, double seconds) =>
        Math.Sin(Math.PI * 2 * (frequency * seconds + slope * seconds * seconds / 2));

    private static void Add(AudioManagerData data, string name, double length, float volume, Func<double, double, double> wave)
    {
        string path = $"{Root}/{name}.wav";
        int size = (int)(length * Rate);
        var random = new System.Random(912);
        using (var stream = File.Create(path))
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + size * 2);
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(Rate);
            writer.Write(Rate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(size * 2);
            for (int i = 0; i < size; i++)
            {
                double time = (double)i / Rate;
                double envelope = Math.Min(1, time * 180) * Math.Pow(1 - time / length, 0.9);
                double sample = Math.Max(-1, Math.Min(1, wave(time, random.NextDouble() * 2 - 1) * envelope));
                writer.Write((short)(sample * short.MaxValue));
            }
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        AudioData sound = data.GetAudioData(name);
        if (sound == null)
        {
            sound = new AudioData { name = name, audioType = AudioType.SFX };
            data.audioData.Add(sound);
        }
        sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        sound.volume = volume;
    }
}
