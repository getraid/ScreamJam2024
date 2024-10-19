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

    List<GeneralGameObjectData> _savedData=new List<GeneralGameObjectData>();
    public void ReloadFromSafe(int saveIndex)
    {
        GeneralGameObjectData dataToLoad = _savedData[saveIndex];

        gameObject.transform.position = dataToLoad.Position;
        gameObject.transform.rotation = dataToLoad.Rotation;
        gameObject.SetActive(dataToLoad.ActiveItself);

        for(int i=0;i<_componentActivityToTrack.Count;i++)
        {
            _componentActivityToTrack[i].enabled = dataToLoad.ComponentActivity[i];
        }
    }

    public void SaveData()
    {
        GeneralGameObjectData saveData;
        saveData.Position=gameObject.transform.position;
        saveData.Rotation = gameObject.transform.rotation;
        saveData.ActiveItself = gameObject.activeSelf;

        saveData.ComponentActivity = _componentActivityToTrack.Select(x=>x.enabled).ToList();

        _savedData.Add(saveData);
    }
}
