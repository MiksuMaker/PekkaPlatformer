using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : SingletonMono<AudioController>
{
    private MusicPlayList playList;
    private AudioMixer mixer;

    //From -80db to 0db
    private const float minDb = -80.0f;
    private const float maxDb = 0.0f;

    private AudioMixerSnapshot noMusic;
    private AudioMixerSnapshot music1FullVolume;
    private AudioMixerSnapshot music2FullVolume;

    private AudioSource music1AudioSource;
    private AudioSource music2AudioSource;
    private AudioSource currentMusicSource;

    private void OnEnable()
    {
        mixer = Resources.Load("GameAudioMixer") as AudioMixer;
        AudioMixerGroup[] musicGroups = mixer.FindMatchingGroups("Music");

        //Get snapshots from mixer
        noMusic = mixer.FindSnapshot("NoMusic");
        music1FullVolume = mixer.FindSnapshot("Music1FullVolume");
        music2FullVolume = mixer.FindSnapshot("Music2FullVolume");

        //Start with no music snapshot...
        noMusic.TransitionTo(0);

        playList = Resources.Load("SoundtrackPlaylist") as MusicPlayList;

        //Add source (and correct output) for Music 1
        music1AudioSource = Instance.gameObject.AddComponent<AudioSource>();
        music1AudioSource.outputAudioMixerGroup = musicGroups[1]; //<-- Music1 in mixer
        music1AudioSource.loop = true;

        //Add source (and correct output) for Music 2
        music2AudioSource = Instance.gameObject.AddComponent<AudioSource>();
        music2AudioSource.outputAudioMixerGroup = musicGroups[2];  //<-- Music2 in mixer
        music2AudioSource.loop = true;

        currentMusicSource = music1AudioSource;

        Debug.Log("playlist first audioclip -> " + playList.Tracks[0].MusicClip);
    }

    private float Percent01ToDb(float percent)
    {
        float dbRange = Mathf.Abs(minDb) + Mathf.Abs(maxDb);
        return minDb + percent * dbRange;
    }

    public bool PlayTrack(string trackName, float sourceVolume = 1f, float transitionTime = 0f, bool loop = true)
    {
        MusicTrack track = playList.Tracks.FirstOrDefault(matchingTrack => matchingTrack.Name == trackName);
        if (track != null)
            PlayTrack(track, sourceVolume, transitionTime, loop);

        return track != null; //<-- to avoid string typos...
    }

    public void PlayTrack(MusicTrack track, float sourceVolume = 1f, float transitionTime = 0f, bool loop = true)
    {        
        if (currentMusicSource == music1AudioSource)
        {
            currentMusicSource = music2AudioSource;
            music2FullVolume.TransitionTo(transitionTime);
        }
        else
        {
            currentMusicSource = music1AudioSource;
            music1FullVolume.TransitionTo(transitionTime);
        }

        currentMusicSource.loop = loop;
        currentMusicSource.volume = sourceVolume;
        currentMusicSource.clip = track.MusicClip;
        currentMusicSource.Play();
    }

    public void StopMusic(float transitionTime = 0f)
    {
        noMusic.TransitionTo(transitionTime);
        //NOTE: And should actually STOP THE MUSIC after transition...
    }

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Percent01ToDb(volume));
    }

    public void SetSfxVolume(float volume)
    {
        mixer.SetFloat("SfxVolume", Percent01ToDb(volume));
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", Percent01ToDb(volume));
    }   
}
