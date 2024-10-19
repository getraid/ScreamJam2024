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

    [field: SerializeField] public bool IsInSafeZone { get; set; } = true;

    private Dictionary<string, AudioClip> audioFilesDict { get; set; }
    public static MusicManager Instance { get; private set; }
    

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private bool isStopping = false;
    private Coroutine musicLoopCoroutine;
    private bool isLevelChanging = false;
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

            if (!IsInSafeZone)
            {
                StartMusic();
            }
        }
    }

    public void StartMusic()
    {
        if (IsInSafeZone)
        {
            Debug.Log("Player is in the safe zone, music will not start.");
            return;
        }

        if (musicLoopCoroutine != null)
        {
            StopCoroutine(musicLoopCoroutine);
        }

        // Start the music sequence with the start audio if specified
        musicLoopCoroutine = StartCoroutine(PlayRandomMusicFromGroup(CurrentAudioGroupIndex, useStart: true));
    }

    // Play Start Sequence, then continue with the random song -> end song loop
    private IEnumerator PlayStartSequenceThenMusic()
    {
        if (StartAudioSequence != null)
        {
            activeSource.clip = StartAudioSequence;
            activeSource.Play();
            yield return new WaitForSeconds(StartAudioSequence.length);
        }

        // Once the start sequence finishes, begin playing random songs and end sequence
        musicLoopCoroutine = StartCoroutine(PlayRandomMusicFromGroup(CurrentAudioGroupIndex));
    }

    public void StopMusic()
    {
        if (musicLoopCoroutine != null && !isStopping)
        {
            isStopping = true;
            StartCoroutine(PlayCurrentLoopThenEndSequence());
        }
    }


   
    private IEnumerator PlayCurrentLoopThenEndSequence()
    {
        // Wait for the current loop to finish
        yield return new WaitWhile(() => activeSource.isPlaying);

        // If there is an end audio sequence, crossfade to it
        if (EndAudioSequence != null)
        {
            yield return StartCoroutine(PlayClipWithCrossfade(currentClip, EndAudioSequence, crossfadeBuffer: 2.0f, fadeDuration: 2.0f));
        }

        isStopping = false;
    }

    public void ChangeLevel(int newAudioGroupIndex)
    {
        if (newAudioGroupIndex == CurrentAudioGroupIndex)
        {
            Debug.Log("Already playing music for the current level.");
            return;
        }

        CurrentAudioGroupIndex = newAudioGroupIndex;
        isLevelChanging = true;

        if (musicLoopCoroutine != null)
        {
            StartCoroutine(SwitchToNewLevelAfterCurrentTrack());
        }
        else
        {
            musicLoopCoroutine = StartCoroutine(PlayRandomMusicFromGroup(CurrentAudioGroupIndex));
        }
    }

    private IEnumerator SwitchToNewLevelAfterCurrentTrack()
    {
        yield return new WaitWhile(() => activeSource.isPlaying);
        isLevelChanging = false;
        musicLoopCoroutine = StartCoroutine(PlayRandomMusicFromGroup(CurrentAudioGroupIndex));
    }

    private IEnumerator PlayRandomMusicFromGroup(int groupIndex, bool useStart = false)
    {
        List<AudioClip> audioClips = GetAudioClipsFromGroup(groupIndex);

        // If the start audio should be used, set it as the current clip
        if (useStart && StartAudioSequence != null)
        {
            currentClip = StartAudioSequence;
        }

        // Start playing the random song sequence
        while (!IsInSafeZone && !isStopping && !isLevelChanging)
        {
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
        // If there is an old clip, set it to the active source
        if (oldClip != null)
        {
            activeSource.clip = oldClip;
            activeSource.volume = 1f;
            activeSource.Play();
        }

        // Set the new clip to the inactive source but don't start it yet
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;

        // If there is an old clip, wait until `crossfadeBuffer` seconds are left before starting the new clip and crossfading
        if (oldClip != null)
        {
            float remainingTime = activeSource.clip.length - activeSource.time;
            if (remainingTime > crossfadeBuffer)
            {
                yield return new WaitForSeconds(remainingTime - crossfadeBuffer);
            }

            // Start the new clip exactly at the crossfadeBuffer time
            inactiveSource.Play();
        }
        else
        {
            // If there's no old clip, just start playing the new clip immediately
            inactiveSource.Play();
        }

        // Now perform the crossfade over the specified duration
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Fade out the active source (old clip) and fade in the inactive source (new clip)
            if (oldClip != null) activeSource.volume = 1 - progress;
            inactiveSource.volume = progress;

            yield return null;
        }

        // Ensure the old clip (active source) is stopped
        if (oldClip != null)
        {
            activeSource.Stop();
            activeSource.volume = 1f; // Reset volume for the next time
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