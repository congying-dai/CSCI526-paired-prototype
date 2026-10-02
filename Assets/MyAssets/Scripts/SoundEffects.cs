using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public enum Sfx
    {
        Shoot,
        Hit,
        Explosion,
        Thunder,
        KeyPickup,
        DoorOpen,
        Denied,
        MysteryGood,
        MysteryBad,
        GhostOn,
        GhostOff,
        Princess,
        Success,
        Reset
    }

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    [Range(0f, 1f)] public float rainVolume = 0.12f;

    private const int Rate = 44100;
    private const float TwoPi = Mathf.PI * 2f;

    private static SoundEffects instance;

    private AudioSource sfxSource;
    private AudioSource rainSource;
    private readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    private readonly Dictionary<Sfx, float> nextAllowedTime = new Dictionary<Sfx, float>();

    private static SoundEffects Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("SoundEffects").AddComponent<SoundEffects>();
            }

            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        SoundEffects unused = Instance;
    }

    public static void Play(Sfx sfx)
    {
        Instance.PlayClip(sfx);
    }

    public static void StartRain()
    {
        Instance.rainSource.Play();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D sound

        rainSource = gameObject.AddComponent<AudioSource>();
        rainSource.playOnAwake = false;
        rainSource.spatialBlend = 0f;
        rainSource.loop = true;
        rainSource.volume = rainVolume;

        BuildClips();
    }

    private void PlayClip(Sfx sfx)
    {
        AudioClip clip;

        if (!clips.TryGetValue(sfx, out clip))
        {
            return;
        }

        float allowed;

        if (nextAllowedTime.TryGetValue(sfx, out allowed) && Time.unscaledTime < allowed)
        {
            return;
        }

        nextAllowedTime[sfx] = Time.unscaledTime + 0.04f;
        sfxSource.PlayOneShot(clip, masterVolume);
    }

    private void BuildClips()
    {
        
        Add(Sfx.Shoot, Finish(Tone(0.09f, p => Mathf.Lerp(950f, 350f, p), Wave.Square, p => Mathf.Exp(-4f * p), 1f), 0.25f));


        Add(Sfx.Hit, Finish(Mix(
            Tone(0.09f, p => Mathf.Lerp(200f, 90f, p), Wave.Sine, p => Mathf.Exp(-6f * p), 1f),
            Noise(0.09f, 3000f, 800f, p => Mathf.Exp(-9f * p), 0.5f)), 0.5f));

        Add(Sfx.Explosion, Finish(Mix(
            Tone(0.9f, p => Mathf.Lerp(90f, 35f, p), Wave.Sine, p => Mathf.Exp(-3.5f * p), 1f),
            Noise(0.9f, 2500f, 150f, p => Mathf.Min(1f, p * 60f) * Mathf.Exp(-4f * p), 1f)), 1f));

        Add(Sfx.Thunder, Finish(Mix(
            Noise(2.4f, 1200f, 90f, p => Mathf.Min(1f, p * 25f) * Mathf.Exp(-1.8f * p), 1f),
            Noise(2.4f, 400f, 60f, p => Mathf.Min(1f, p * 6f) * Mathf.Exp(-1.2f * p), 1f)), 0.9f));

        Add(Sfx.KeyPickup, Finish(Notes(new[] { 880f, 1320f }, 0.12f, Wave.Sine, 3f), 0.5f));

 
        Add(Sfx.DoorOpen, Finish(Concat(
            Tone(0.45f, p => Mathf.Lerp(70f, 130f, p) + 6f * Mathf.Sin(p * 60f), Wave.Saw, p => Mathf.Sin(p * Mathf.PI) * 0.8f, 1f),
            Tone(0.15f, p => Mathf.Lerp(120f, 50f, p), Wave.Sine, p => Mathf.Exp(-7f * p), 1f)), 0.6f));


        Add(Sfx.Denied, Finish(Concat(
            Tone(0.09f, p => 150f, Wave.Square, p => 1f - p, 1f),
            Tone(0.12f, p => 120f, Wave.Square, p => 1f - p, 1f)), 0.35f));

     
        Add(Sfx.MysteryGood, Finish(Notes(new[] { 523f, 659f, 784f, 1047f }, 0.09f, Wave.Sine, 3.5f), 0.5f));

        Add(Sfx.MysteryBad, Finish(Tone(0.6f, p => Mathf.Lerp(420f, 80f, p) + 12f * Mathf.Sin(p * 90f), Wave.Saw, p => 1f - p, 1f), 0.45f));

        Add(Sfx.GhostOn, Finish(Mix(
            Tone(0.6f, p => Mathf.Lerp(250f, 900f, p), Wave.Sine, p => Mathf.Sin(p * Mathf.PI), 0.6f),
            Noise(0.6f, 500f, 4000f, p => Mathf.Sin(p * Mathf.PI), 0.7f)), 0.4f));

        Add(Sfx.GhostOff, Finish(Mix(
            Tone(0.5f, p => Mathf.Lerp(700f, 200f, p), Wave.Sine, p => Mathf.Sin(p * Mathf.PI), 0.6f),
            Noise(0.5f, 3500f, 400f, p => Mathf.Sin(p * Mathf.PI), 0.7f)), 0.35f));

        Add(Sfx.Princess, Finish(Notes(new[] { 659f, 784f, 988f }, 0.16f, Wave.Sine, 2.5f), 0.5f));

        Add(Sfx.Success, Finish(Concat(
            Notes(new[] { 523f, 659f, 784f }, 0.14f, Wave.Square, 3f),
            Tone(0.7f, p => 1047f, Wave.Square, p => Mathf.Exp(-2.5f * p), 1f)), 0.4f));

        Add(Sfx.Reset, Finish(Tone(0.35f, p => Mathf.Lerp(900f, 120f, p), Wave.Saw, p => 1f - p, 1f), 0.45f));

        rainSource.clip = MakeClip("Rain", Finish(Noise(4f, 6000f, 6000f, p => 1f, 1f), 1f));
    }

    private void Add(Sfx sfx, float[] samples)
    {
        clips[sfx] = MakeClip(sfx.ToString(), samples);
    }

    private static AudioClip MakeClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, Rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private enum Wave { Sine, Square, Saw }


    private static float[] Tone(float duration, Func<float, float> freq, Wave wave, Func<float, float> env, float volume)
    {
        int n = (int)(duration * Rate);
        float[] data = new float[n];
        float phase = 0f;

        for (int i = 0; i < n; i++)
        {
            float p = (float)i / n;
            phase = Mathf.Repeat(phase + freq(p) / Rate, 1f);

            float s;

            switch (wave)
            {
                case Wave.Square: s = phase < 0.5f ? 0.6f : -0.6f; break;
                case Wave.Saw: s = (phase * 2f - 1f) * 0.7f; break;
                default: s = Mathf.Sin(phase * TwoPi); break;
            }

            data[i] = s * env(p) * volume;
        }

        return data;
    }

    private static float[] Noise(float duration, float cutStart, float cutEnd, Func<float, float> env, float volume)
    {
        int n = (int)(duration * Rate);
        float[] data = new float[n];
        float y = 0f;

        for (int i = 0; i < n; i++)
        {
            float p = (float)i / n;
            float cutoff = Mathf.Lerp(cutStart, cutEnd, p);
            float a = 1f - Mathf.Exp(-TwoPi * cutoff / Rate);
            y += a * (UnityEngine.Random.value * 2f - 1f - y);
            data[i] = y * env(p) * volume;
        }

        return data;
    }

    private static float[] Notes(float[] freqs, float noteDuration, Wave wave, float decay)
    {
        float[][] parts = new float[freqs.Length][];

        for (int i = 0; i < freqs.Length; i++)
        {
            float f = freqs[i];
            parts[i] = Tone(noteDuration, p => f, wave, p => Mathf.Exp(-decay * p), 1f);
        }

        return Concat(parts);
    }

    private static float[] Concat(params float[][] parts)
    {
        int total = 0;

        foreach (float[] part in parts)
        {
            total += part.Length;
        }

        float[] result = new float[total];
        int offset = 0;

        foreach (float[] part in parts)
        {
            Array.Copy(part, 0, result, offset, part.Length);
            offset += part.Length;
        }

        return result;
    }

    private static float[] Mix(params float[][] layers)
    {
        int length = 0;

        foreach (float[] layer in layers)
        {
            length = Mathf.Max(length, layer.Length);
        }

        float[] result = new float[length];

        foreach (float[] layer in layers)
        {
            for (int i = 0; i < layer.Length; i++)
            {
                result[i] += layer[i];
            }
        }

        return result;
    }

    private static float[] Finish(float[] data, float peak)
    {
        float max = 0.0001f;

        foreach (float s in data)
        {
            max = Mathf.Max(max, Mathf.Abs(s));
        }

        int fade = Mathf.Min(200, data.Length / 4);

        for (int i = 0; i < data.Length; i++)
        {
            float gain = peak / max;

            if (i < fade) gain *= (float)i / fade;
            if (i > data.Length - fade) gain *= (float)(data.Length - i) / fade;

            data[i] *= gain;
        }

        return data;
    }
}
