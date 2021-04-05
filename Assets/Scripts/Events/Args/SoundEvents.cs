using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayMusicEvent
{
    public AudioClip clip;
    public float volume;
    public float transitionTime;

    public PlayMusicEvent(AudioClip _clip, float _volume = 0.5f, float _transition = 1.0f)
    {
        clip = _clip;
        volume = _volume;
        transitionTime = _transition;
    }
}

public class StopMusicEvent
{
    public float fade;

    public StopMusicEvent(float _fade = 1.0f)
    {
        fade = _fade;
    }
}

public class IsPlayingMusicEvent
{
    public AudioClip clip;
    public bool isPlaying;

    public IsPlayingMusicEvent(AudioClip _clip)
    {
        clip = _clip;
        isPlaying = false;
    }
}

public class PlaySoundEvent
{
    public AudioClip clip;
    public float volume;
    public bool force;

    public PlaySoundEvent(AudioClip _clip, float _volume = 0.5f, bool _force = false)
    {
        clip = _clip;
        volume = _volume;
        force = _force;
    }
}

public class IsPlayingSoundEvent
{
    public AudioClip clip;
    public bool isPlaying;

    public IsPlayingSoundEvent(AudioClip _clip)
    {
        clip = _clip;
        isPlaying = false;
    }
}

public class PlaySoundLoopEvent
{
    public AudioClip clip;
    public float volume;
    public float fade;

    public int id;

    public PlaySoundLoopEvent(AudioClip _clip, float _volume = 0.5f, float _fade = 0.5f)
    {
        clip = _clip;
        volume = _volume;
        fade = _fade;

        id = -1;
    }
}

public class StopSoundLoopEvent
{
    public int id;
    public float fade;

    public StopSoundLoopEvent(int _id, float _fade = 0.5f)
    {
        id = _id;
        fade = _fade;
    }
}
