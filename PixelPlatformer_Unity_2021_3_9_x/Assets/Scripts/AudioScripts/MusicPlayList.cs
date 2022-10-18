using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Music Playlist", menuName = "Music Playlist", order = 54)]
public class MusicPlayList : ScriptableObject
{
    [SerializeField]
    private List<MusicTrack> tracks;

    public List<MusicTrack> Tracks { get { return tracks; } }
}

[Serializable]
public class MusicTrack
{
    public string Name;
    public AudioClip MusicClip;
}



