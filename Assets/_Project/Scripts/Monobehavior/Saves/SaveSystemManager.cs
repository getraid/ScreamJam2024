using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveSystemManager : MonoBehaviour
{
    List<DateTime> _savedDateStamps=new List<DateTime>();
    bool _savedBefore = false;
    VoiceLineDataSO _voiceLineActiveOnSave;
    public static SaveSystemManager Instance { get; private set; }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Save();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ReloadLastSave();
        }
        
#endif
    }

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Save()
    {
        StartCoroutine(SaveWithSlightDelay());

        JournalManager.Instance.EnableCheckpointLoad();
        IEnumerator SaveWithSlightDelay()
        {
            yield return new WaitForSeconds(1);         //Slight delay so we correctly estimate if the voice line is playing or not

            List<ISaveable> allSaveable = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();
            _voiceLineActiveOnSave = VoiceLineManager.Instance.CurrentVoiceLinePlaying;

            DateTime timeOfSave = DateTime.Now;
            _savedDateStamps.Add(timeOfSave);
            allSaveable.ForEach(x => x.SaveData(timeOfSave));

            _savedBefore = true;
            Debug.Log($"Game saved in {timeOfSave:dd.MM.yyyy HH:mm:ss}!");
        }
        
    }

    public void ReloadLastSave()
    {
        if(!_savedBefore)
        {
            Debug.LogError("Nothing has been saved yet, cannot reload last save! Save with F1");
            return;
        }
        DateTime lastSave = _savedDateStamps.Last();

        List<ISaveable> allSaveable = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();
        allSaveable.ForEach(x => x.ReloadFromSafe(lastSave));

        if (_voiceLineActiveOnSave)
            VoiceLineManager.Instance.PlayVoiceLine(_voiceLineActiveOnSave, null);

        Debug.Log($"Game save reloaded! From {lastSave:dd.MM.yyyy HH:mm:ss}");

    }
}
