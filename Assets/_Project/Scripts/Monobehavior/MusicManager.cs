using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AudioGroup
{
    public int LevelIndex = 0;
    public List<AudioTrackNames> AudioTracksList;
}

public enum AudioTrackNames
{
    cello,
    clock,
    deep_blue,
    deeper,
    ghost_church,
    happy_birthday,
    monster,
    string_tension
}

public class MusicManager : MonoBehaviour
{
    [field: SerializeField] public List<AudioClip> AudioFiles { get; set; }

    [field: SerializeField] public AudioSource AudioSourceA { get; set; }
    [field: SerializeField] public AudioSource AudioSourceB { get; set; }

    [field: SerializeField] public AudioClip StartAudioSequence { get; set; }
    [field: SerializeField] public AudioClip EndAudioSequence { get; set; }

    [field: SerializeField] public List<AudioGroup> AudioGroups { get; set; }
    [field: SerializeField] public int CurrentAudioGroupIndex { get; set; } = 0;

    [field: SerializeField] public bool IsInQuietZone { get; set; } = true;

    private Dictionary<string, AudioClip> audioFilesDict { get; set; }
    public static MusicManager Instance { get; private set; }

    float _maxVolume = 1;
    bool _isMusicPlaying = false;

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private Coroutine musicLoopCoroutine;
    private AudioClip currentClip = null; // Store the currently playing clip

    void Start()
    {
        if (AudioSourceA == null || AudioSourceB == null)
            throw new NullReferenceException("Both AudioSources must be assigned.");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            audioFilesDict = new Dictionary<string, AudioClip>();
            PopulateDict();

            Instance = this;

            activeSource = AudioSourceA;
            inactiveSource = AudioSourceB;

            if (!IsInQuietZone)
            {
                TryStartMusic();
            }
        }
    }
    private void FixedUpdate()
    {
        if (IsInQuietZone)
            _maxVolume = Mathf.Max(0, _maxVolume - 0.003f);
        else
            _maxVolume = Mathf.Min(1, _maxVolume + 0.002f);

        activeSource.volume = _maxVolume;
    }

    public void ChangeMusicGroup(int groupNumber)
    {
        CurrentAudioGroupIndex = groupNumber;
    }

    public void TryStartMusic()
    {
        if(_isMusicPlaying)
            return;
        
        if (IsInQuietZone)
        {
            Debug.Log("Player is in the quiet zone, music will not start.");
            return;
        }

        if (musicLoopCoroutine != null)
        {
            StopCoroutine(musicLoopCoroutine);
        }

        _isMusicPlaying = true;
        // Start the music sequence with the start audio if specified
        musicLoopCoroutine = StartCoroutine(PlayRandomMusicFromGroup(useStart: true));
    }

    public void ChangeLevel(int newAudioGroupIndex)
    {
        if (newAudioGroupIndex == CurrentAudioGroupIndex)
        {
            Debug.Log("Already playing music for the current level.");
            return;
        }
    }

    private IEnumerator PlayRandomMusicFromGroup(bool useStart = false)
    {

        // If the start audio should be used, set it as the current clip
        if (useStart && StartAudioSequence != null)
        {
            currentClip = StartAudioSequence;
        }

        // Start playing the random song sequence
        while (true)
        {
            List<AudioClip> audioClips = GetAudioClipsFromGroup(CurrentAudioGroupIndex);

            if (audioClips.Count > 0)
            {
                // Select a new random song if the current clip is not the start sequence
                AudioClip newClip;
                if (currentClip == null || currentClip == StartAudioSequence)
                {
                    newClip = audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
                }
                else
                {
                    // Avoid playing the same clip consecutively
                    do
                    {
                        newClip = audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
                    } while (newClip == currentClip);
                }

                // Crossfade from the current clip to the new clip
                yield return StartCoroutine(PlayClipWithCrossfade(currentClip, newClip, crossfadeBuffer: 2.0f, fadeDuration: 2.0f));

                // Update the current clip to the new clip
                currentClip = newClip;
            }
            else
            {
                Debug.LogWarning("No audio clips available for the current group.");
                yield break;
            }
        }
    }

    private IEnumerator PlayClipWithCrossfade(AudioClip oldClip, AudioClip newClip, float crossfadeBuffer = 2.0f, float fadeDuration = 2.0f)
    {  
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;

        // If there is an old clip, wait until `crossfadeBuffer` seconds are left before starting the new clip and crossfading
        if (oldClip != null && activeSource.clip!=null)
        {
            float remainingTime = activeSource.clip.length - activeSource.time;
            if (remainingTime > crossfadeBuffer)
            {
                yield return new WaitForSeconds(remainingTime - crossfadeBuffer);
            }

            // Start the new clip exactly at the crossfadeBuffer time

            if(!inactiveSource.isPlaying)
                inactiveSource.Play();
        }
        else
        {
            // If there's no old clip, just start playing the new clip immediately
            if (!inactiveSource.isPlaying)
                inactiveSource.Play();
        }

        // Now perform the crossfade over the specified duration
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Fade out the active source (old clip) and fade in the inactive source (new clip)
            if (oldClip != null) activeSource.volume = Math.Clamp(1 - progress,0, _maxVolume);
            inactiveSource.volume = Math.Clamp(progress,0,_maxVolume);

            yield return null;
        }

        // Ensure the old clip (active source) is stopped
        if (oldClip != null)
        {
            activeSource.Stop();
            activeSource.volume = _maxVolume; // Reset volume for the next time
        }

        // Swap the active and inactive sources
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }
    public List<AudioClip> GetAudioClipsFromGroup(int groupIndex)
    {
        List<AudioTrackNames> currentList =
            AudioGroups.FirstOrDefault(el => el.LevelIndex == groupIndex)?.AudioTracksList;
        List<AudioClip> audioClips = new List<AudioClip>();
        foreach (var item in currentList)
        {
            audioClips.Add(GetAudioClip(item));
        }

        return audioClips;
    }

    public AudioClip GetAudioClip(AudioTrackNames audioTrack)
    {
        var audioNameSerialized = audioTrack.ToString().Replace("_", " ");
        var successful = audioFilesDict.TryGetValue(audioNameSerialized, out AudioClip audioClip);
        if (!successful || audioClip == null)
            throw new NullReferenceException("Audio clip not found");
        return audioClip;
    }

    private void PopulateDict()
    {
        for (var index = 0; index < AudioFiles.Count; index++)
        {
            var audioFile = AudioFiles[index];
            audioFilesDict.Add(audioFile.name, audioFile);
        }
    }
}