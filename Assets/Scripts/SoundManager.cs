using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this);
            Debug.LogWarning("Multiple sound system in scene not allowed");
        }
        else
        {
            instance = this;
        }
    }

    #region Wrapper
    public void Play(AudioClip clip)
    {
        Play(clip);
    }
    public void Play(AudioClip clip, float volume)
    {
        Play(clip, false, 1, volume: volume);
    }
    public void Play(AudioClip clip, float volume, float pitch)
    {
        Play(clip, false, volume, pitch);
    }
    public void Play(AudioClip clip, float volume, float minPitch, float maxPitch)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        Play(clip, false, volume, pitch);
    }
    public void Play(AudioClip clip, float minVolume, float maxVolume, float minPitch, float maxPitch)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        float volume = Random.Range(minVolume, maxVolume);
        Play(clip, false, volume, pitch);
    }
    public void PlayInLoop(AudioClip clip)
    {
        Play(clip, true);
    }
    public void PlayList(AudioClip[] clip)
    {
        for (int i = 0; i < clip.Length; i++)
        {
            Play(clip[i], true);
        }
    }
    #endregion

    public static void Play(AudioClip clip, bool loop = false, float pitch = 1, float volume = 1)
    {
        if(!instance)
        {
            Debug.LogWarning("Cannot play sounds without sound system instance");
            return;
        }

        AudioSource source = instance.gameObject.AddComponent<AudioSource>();
        source.hideFlags = HideFlags.HideInInspector;
        source.clip = clip;
        source.playOnAwake = false;
        source.loop = loop;
        source.pitch = pitch;
        source.volume = volume;
        source.Play();

        if (!loop)
            instance.StartCoroutine(instance.DestroySourceOnEnd(source));
    }

    IEnumerator DestroySourceOnEnd(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying == true);
        Destroy(source);
    }


}
