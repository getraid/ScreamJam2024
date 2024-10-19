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

    // Start Music: This method now starts the initial StartAudioSequence
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

        // Start by playing the start sequence
        musicLoopCoroutine = StartCoroutine(PlayStartSequenceThenMusic());
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
        yield return new WaitWhile(() => activeSource.isPlaying);

        if (EndAudioSequence != null)
        {
            activeSource.clip = EndAudioSequence;
            activeSource.Play();
            yield return new WaitForSeconds(EndAudioSequence.length);
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

    private IEnumerator PlayRandomMusicFromGroup(int groupIndex)
    {
        List<AudioClip> audioClips = GetAudioClipsFromGroup(groupIndex);
        
        AudioClip prevClip = null;
        
        // Start playing the random song sequence with end crossfade
        while (!IsInSafeZone && !isStopping && !isLevelChanging)
        {
            if (audioClips.Count > 0)
            {
                // Play random song
                AudioClip newClip = audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
                
                yield return StartCoroutine(PlayClipWithCrossfade(newClip, prevClip));
                prevClip = newClip;

                // After the random song, crossfade to the end audio sequence
                // yield return StartCoroutine(PlayClipWithCrossfade(EndAudioSequence));

                // Continue to loop back to random song
            }
            else
            {
                Debug.LogWarning("No audio clips available for the current group.");
                yield break;
            }
        }
    }
    
    // Play the current clip and initiate the crossfade 2 seconds before it ends
    private IEnumerator PlayClipWithCrossfade(AudioClip clip, AudioClip next = null, float crossfadeBuffer = 5.0f, float fadeDuration = 5.0f)
    {
        inactiveSource.clip = clip;
        inactiveSource.Play();
        
        // Wait until there are only `crossfadeBuffer` seconds left before starting the crossfade
        yield return new WaitForSeconds(clip.length - crossfadeBuffer);

        if (next != null)
        {
            activeSource.volume = 0f;
            activeSource.clip = next;
            activeSource.Play();    
        }

        // Now perform the crossfade over the `fadeDuration`
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Fade out the active source and fade in the inactive source
            inactiveSource.volume = 1 - progress;
            activeSource.volume = progress;
            if (progress >= 0.99f)
            {
                inactiveSource.Stop();
                inactiveSource.volume = 0f;
            }
                

            yield return null;
        }

        // Ensure the old source stops and reset volumes
        // activeSource.Stop();
        // activeSource.volume = 1f;

        // Swap the active and inactive sources
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }
    

    [Obsolete("Crossfade is fired too quickly and sounds funky use PlayClipWithCrossfade")]
    private IEnumerator CrossfadeToNextClip(AudioClip nextClip, float fadeDuration = 1.0f)
    {
        // Assign the new clip to the inactive source
        inactiveSource.clip = nextClip;
        inactiveSource.Play();

        float timer = 0f;

        // Perform the crossfade (fade out active source, fade in inactive source)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Smoothly reduce the volume of the active source (fade out)
            activeSource.volume = 1 - progress;

            // Smoothly increase the volume of the inactive source (fade in)
            inactiveSource.volume = progress;

            yield return null;
        }

        // Ensure that the crossfade completes
        activeSource.Stop();  // Stop the old clip
        activeSource.volume = 1f;  // Reset volume for the next crossfade

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