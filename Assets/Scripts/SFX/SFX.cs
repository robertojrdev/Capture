using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFX : MonoBehaviour
{   private static SFX instance;

    private AudioClip[] footstepClip_Grass;
    private AudioClip[] footstepClip_Wood;
    private AudioClip[] footstepClip_Water;

    private List<AudioMixerGroup> audioMixerGroups;

    public static SFX Instance
    {
        get
        {
            if (instance != null)
                return instance;
            else
            {
                instance = new GameObject().AddComponent<SFX>();
                instance.LoadAudioMixerGroups();
                instance.LoadFootStepsClips();
                return instance;
            }
        }
    }
    public enum MixerGroup
    {
        Music, Sounds, Master
    }
    public enum FootstepType
    {
        Grass, Wood, Water
    }

    public static void Play(string name, SFXProfile profile, MixerGroup group = MixerGroup.Master)
    {
        AudioClip clip = FindAudioClip(name);
        if (clip == null)
        {
            Debug.LogWarning("No sound named '" + name + "'");
            return;
        }
        
        AudioSource source = Instance.gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = Instance.audioMixerGroups.Find(x => x.name == group.ToString());
        source.clip = clip;
        source.hideFlags = HideFlags.HideInInspector;
        profile.SetSource(source);
        source.Play();
        if (!source.loop)
            Instance.StartCoroutine(DestroySourceOnFinish(source));

        Debug.Log("Sound '" + name + "' played");
    }

    public static AudioClip FindAudioClip(string name)
    {
        return Resources.Load("Sounds/General/" + name, typeof(AudioClip)) as AudioClip;
    }
    
    private static IEnumerator DestroySourceOnFinish(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        Destroy(source);
    }

    private void LoadFootStepsClips()
    {
        Object[] grass = Resources.LoadAll("Sounds/Footsteps/Grass", typeof(AudioClip));
        Object[] Wood = Resources.LoadAll("Sounds/Footsteps/Wood", typeof(AudioClip));
        Object[] Water = Resources.LoadAll("Sounds/Footsteps/Water", typeof(AudioClip));

        footstepClip_Grass = new AudioClip[grass.Length];
        for (int i = 0; i < grass.Length; i++)
            footstepClip_Grass[i] = grass[i] as AudioClip;

        footstepClip_Wood = new AudioClip[Wood.Length];
        for (int i = 0; i < Wood.Length; i++)
            footstepClip_Wood[i] = Wood[i] as AudioClip;

        footstepClip_Water = new AudioClip[Water.Length];
        for (int i = 0; i < Water.Length; i++)
            footstepClip_Water[i] = Water[i] as AudioClip;
    }

    private void LoadAudioMixerGroups()
    {
        Object[] mixerGroupsObj = Resources.LoadAll("Sounds/AudioMixer", typeof(AudioMixerGroup));
        Debug.Log(mixerGroupsObj.Length + " mixer groups loaded");
        audioMixerGroups = new List<AudioMixerGroup>();

        for (int i = 0; i < mixerGroupsObj.Length; i++)
            audioMixerGroups.Add(mixerGroupsObj[i] as AudioMixerGroup);
    }

    public static void PlayFootStep(FootstepType type, float volume = 1, Vector2? pitchMinMax = null)
    {
        AudioClip[] clips = null;
        switch (type)
        {
            case FootstepType.Grass:
                clips = Instance.footstepClip_Grass;
                break;
            case FootstepType.Wood:
                clips = Instance.footstepClip_Wood;
                break;
            case FootstepType.Water:
                clips = Instance.footstepClip_Water;
                break;
        }

        if(clips == null)
        {
            Debug.Log("No footsteps to play of type: " + type.ToString());
            return;
        }

        SFXProfile profile = new SFXProfile(volume: volume, randomPitch: true, minMaxPitch: pitchMinMax);
        AudioSource source = Instance.gameObject.AddComponent<AudioSource>();
        profile.SetSource(source);
        source.clip = clips[new System.Random().Next(0, clips.Length)];
        source.outputAudioMixerGroup = Instance.audioMixerGroups.Find(x => x.name == MixerGroup.Sounds.ToString());
        source.Play();
        Instance.StartCoroutine(DestroySourceOnFinish(source));
    }

    public static void ChangeVolume(MixerGroup mixer, float value)
    {
        AudioMixerGroup group = Instance.audioMixerGroups.Find(x => x.name == mixer.ToString());
        if(group)
        {
            group.audioMixer.SetFloat(mixer.ToString() + "_Volume", value);
        }
    }

    public static float GetVolume(MixerGroup mixer)
    {
        AudioMixerGroup group = Instance.audioMixerGroups.Find(x => x.name == mixer.ToString());
        if(group)
        {
            float val;
            group.audioMixer.GetFloat(mixer.ToString() + "_Volume", out val);
            return val;
        }

        return 0;
    }
}

public struct SFXProfile
{
    public bool playOnAwake;
    public bool loop;
    public bool randomPitch;
    public Vector2 minMaxPitch;
    public float volume;
    public float pitch;

    public SFXProfile(bool playOnAwake = true, bool loop = false, bool randomPitch = false, float volume = 1, float pitch = 1, Vector2? minMaxPitch = null)
    {
        this.playOnAwake = playOnAwake;
        this.loop = loop;
        this.randomPitch = randomPitch;
        this.minMaxPitch = minMaxPitch == null ? Vector2.one : minMaxPitch.Value;
        this.volume = volume;
        this.pitch = pitch;
    }

    public void SetSource(AudioSource source)
    {
        source.playOnAwake = playOnAwake;
        source.loop = loop;
        source.volume = volume;
        source.pitch = randomPitch ? Random.Range(minMaxPitch.x, minMaxPitch.y) : pitch;
    }
}
