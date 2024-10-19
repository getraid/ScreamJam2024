using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveSystemManager : MonoBehaviour
{
    int _lastSavedIndex = 0;
    bool _savedBefore = false;
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

    void Save()
    {
        List<ISaveable> allSaveable= FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();
        allSaveable.ForEach(x => x.SaveData());

        _savedBefore = true;
        Debug.Log("Game saved!");
    }
    void ReloadLastSave()
    {
        if(!_savedBefore)
        {
            Debug.LogError("Nothing has been saved yet, cannot reload last save! Save with F1");
            return;
        }
        List<ISaveable> allSaveable = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();
        allSaveable.ForEach(x => x.ReloadFromSafe(_lastSavedIndex));
        _lastSavedIndex = Mathf.Max(0, _lastSavedIndex - 1);

        Debug.Log($"Game save reloaded! {_lastSavedIndex}");

    }
}
