using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneralGameObjectSave : MonoBehaviour, ISaveable
{
    

    struct GeneralGameObjectData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool ActiveItself;
        public List<bool> ComponentActivity;
    }
    [SerializeField] List<MonoBehaviour> _componentActivityToTrack;

    Dictionary<DateTime,GeneralGameObjectData> _savedData= new Dictionary<DateTime, GeneralGameObjectData>();
    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_savedData.ContainsKey(saveDateStamp))
        {
            GeneralGameObjectData dataToLoad = _savedData[saveDateStamp];

            gameObject.transform.position = dataToLoad.Position;
            gameObject.transform.rotation = dataToLoad.Rotation;
            gameObject.SetActive(dataToLoad.ActiveItself);

            for (int i = 0; i < _componentActivityToTrack.Count; i++)
            {
                _componentActivityToTrack[i].enabled = dataToLoad.ComponentActivity[i];
            }
        }
        else
            Destroy(gameObject);
        
    }

    public void SaveData(DateTime saveDateStamp)
    {
        GeneralGameObjectData saveData;
        saveData.Position=gameObject.transform.position;
        saveData.Rotation = gameObject.transform.rotation;
        saveData.ActiveItself = gameObject.activeSelf;

        saveData.ComponentActivity = _componentActivityToTrack.Select(x=>x.enabled).ToList();

        _savedData.Add(saveDateStamp, saveData);
    }
}
