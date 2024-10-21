using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public enum SFXType
    {
        None,
      HeavyBreathing_1,
      HeavyBreathing_2,
      Inhaler,
      ChurchBell,
      PaperGrab,
      Beep,
      WalkieTalkie,
      WoodGrab,
      Suspense,
      GlassBreak
    }

   [System.Serializable]
    public struct SFXClip
    {
        [field: SerializeField] public SFXType Type { get; set; }
        [field: SerializeField] public AudioClip Clip { get; set; }
    }

    [field: SerializeField] public SFXClip[] SfxClips { get; set; }
    [field: SerializeField] public int PoolSize { get; set; } = 10;

    private List<AudioSource> audioSourcePool;
    private Dictionary<SFXType, AudioClip> sfxClipDictionary;
    private HashSet<SFXType> currentlyPlaying;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        InitializeSFXDictionary();
        InitializeAudioSourcePool();

        currentlyPlaying = new HashSet<SFXType>();
    }

    private void InitializeSFXDictionary()
    {
        sfxClipDictionary = new Dictionary<SFXType, AudioClip>();
        foreach (var sfxClip in SfxClips)
        {
            if (!sfxClipDictionary.ContainsKey(sfxClip.Type))
            {
                sfxClipDictionary.Add(sfxClip.Type, sfxClip.Clip);
            }
        }
    }

    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new List<AudioSource>();

        for (int i = 0; i < PoolSize; i++)
        {
            GameObject audioSourceObject = new GameObject($"AudioSource_{i}");
            audioSourceObject.transform.parent = transform;

            AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Default to 2D audio
            audioSourcePool.Add(audioSource);
        }
    }

    public void PlaySFX(SFXType type,float volume, bool playOnce = true)
    {
        PlaySFX(type,Vector3.zero, volume, playOnce, false);
    }
    public void PlaySFX(SFXType type, Vector3 position,float volume, bool playOnce = true, bool use3DAudio = false)
    {
        if (type == SFXType.None)
            return;

        // If playOnce is true and the sound effect is already playing, do not play it again
        if (playOnce && currentlyPlaying.Contains(type))
        {
            return;
        }

        if (!sfxClipDictionary.TryGetValue(type, out AudioClip clip))
        {
            Debug.LogWarning($"SFXManager: No audio clip found for SFXType {type}");
            return;
        }

        AudioSource availableSource = GetAvailableAudioSource();

        if (availableSource != null)
        {
            availableSource.transform.position = position;
            availableSource.clip = clip;
            availableSource.volume = volume;
            availableSource.spatialBlend = use3DAudio ? 1f : 0f; // Set to 3D if use3DAudio is true, otherwise 2D
            availableSource.Play();

            if (playOnce)
            {
                currentlyPlaying.Add(type);
                StartCoroutine(RemoveFromCurrentlyPlaying(type, clip.length));
            }
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var audioSource in audioSourcePool)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        return null;
    }

    private System.Collections.IEnumerator RemoveFromCurrentlyPlaying(SFXType type, float delay)
    {
        yield return new WaitForSeconds(delay);
        currentlyPlaying.Remove(type);
    }
}